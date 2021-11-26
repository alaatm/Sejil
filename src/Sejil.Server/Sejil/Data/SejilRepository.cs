// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Dapper;
using Sejil.Configuration;
using Sejil.Data.Query;
using Sejil.Models;
using Serilog.Events;

namespace Sejil.Data;

public abstract class SejilRepository : ISejilRepository
{
    private bool _dbInitialized;

    protected ISejilSettings Settings { get; }
    protected string ConnectionString { get; }

    public SejilRepository(ISejilSettings settings, string connectionString)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    protected abstract string LogTableName { get; }

    protected abstract string LogPropertyTableName { get; }

    protected abstract string LogQueryTableName { get; }

    protected abstract Task InitializeDatabaseAsync();

    protected abstract DbConnection GetConnection();

    protected abstract string GetPaginSql(int offset, int take);

    protected abstract string GetDateTimeOffsetSql(int value, string unit);

    private async Task<DbConnection> GetConnectionAsync()
    {
        if (!_dbInitialized)
        {
            _dbInitialized = true;
            await InitializeDatabaseAsync();
        }
        var conn = GetConnection();
        await conn.OpenAsync();
        return conn;
    }

    public async Task<IEnumerable<LogQuery>> GetSavedQueriesAsync()
    {
        const string Sql = "SELECT * FROM {0}";

        using var conn = await GetConnectionAsync();
        return await conn.QueryAsync<LogQuery>(string.Format(CultureInfo.InvariantCulture, Sql, LogQueryTableName));
    }

    public async Task<bool> SaveQueryAsync(LogQuery logQuery)
    {
        const string Sql = "INSERT INTO {0} (name, query) VALUES (@name, @query)";

        using var conn = await GetConnectionAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = string.Format(CultureInfo.InvariantCulture, Sql, LogQueryTableName);
        cmd.CommandType = CommandType.Text;
        cmd.AddParameterWithValue("@name", logQuery.Name);
        cmd.AddParameterWithValue("@query", logQuery.Query);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteQueryAsync(string queryName)
    {
        const string Sql = "DELETE FROM {0} WHERE name = @name";

        using var conn = await GetConnectionAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = string.Format(CultureInfo.InvariantCulture, Sql, LogQueryTableName);
        cmd.CommandType = CommandType.Text;
        cmd.AddParameterWithValue("@name", queryName);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<IEnumerable<LogEntry>> GetEventsPageAsync(int page, DateTime? startingTimestamp, LogQueryFilter queryFilter)
    {
        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Argument must be greater than zero.");
        }

        var sql = GetPagedLogEntriesSql(page, Settings.PageSize, startingTimestamp, queryFilter);

        using var conn = await GetConnectionAsync();
        var lookup = new Dictionary<string, LogEntry>();

        await conn.QueryAsync<LogEntry, LogEntryProperty, LogEntry>(sql, (l, p) =>
        {
            if (!lookup.TryGetValue(l.Id, out var logEntry))
            {
                lookup.Add(l.Id, logEntry = l);
            }

            if (p is not null)
            {
                logEntry.Properties.Add(p);
            }

            return logEntry;
        });

        return lookup.Values;
    }

    public async Task InsertEventsAsync(IEnumerable<LogEvent> events)
    {
        using var conn = await GetConnectionAsync();
        using var tran = conn.BeginTransaction();
        using (var cmdLogEntry = CreateLogEntryInsertCommand(conn, tran))
        using (var cmdLogEntryProperty = CreateLogEntryPropertyInsertCommand(conn, tran))
        {
            foreach (var logEvent in events)
            {
                // Do not log events that were generated from browsing Sejil URL.
                if (logEvent.Properties.Any(p => (p.Key == "RequestPath" || p.Key == "Path") &&
                    p.Value.ToString().Contains(Settings.Url, StringComparison.Ordinal)))
                {
                    continue;
                }

                var logId = await InsertLogEntryAsync(cmdLogEntry, logEvent);
                foreach (var property in logEvent.Properties)
                {
                    await InsertLogEntryPropertyAsync(cmdLogEntryProperty, logId, property);
                }
            }
        }
        await tran.CommitAsync();
    }

    internal string GetPagedLogEntriesSql(int page, int pageSize, DateTime? startingTimestamp, LogQueryFilter queryFilter)
    {
        var timestampWhereClause = TimestampWhereClause();
        var queryWhereClause = QueryWhereClause();

        return string.Format(CultureInfo.InvariantCulture,
$@"SELECT l.*, p.* from
(
    SELECT * FROM {{0}}
    {timestampWhereClause}
    {queryWhereClause}{FiltersWhereClause()}
    ORDER BY timestamp DESC
    {GetPaginSql((page - 1) * pageSize, pageSize)}
) l
LEFT JOIN {{1}} p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name",
        LogTableName, LogPropertyTableName);

        string TimestampWhereClause()
        {
            var hasDateFilter = queryFilter.DateFilter is not null || queryFilter.DateRangeFilter is not null;

            if (startingTimestamp.HasValue || hasDateFilter)
            {
                var sql = new StringBuilder();
                sql.Append("WHERE (");

                if (startingTimestamp.HasValue)
                {
                    sql.AppendFormat(CultureInfo.InvariantCulture, "timestamp <= '{0:yyyy-MM-dd HH:mm:ss.fff}'", startingTimestamp.Value);
                }
                if (startingTimestamp.HasValue && hasDateFilter)
                {
                    sql.Append(" AND ");
                }
                if (hasDateFilter)
                {
                    sql.Append(BuildDateFilter(queryFilter));
                }

                sql.Append(')');
                return sql.ToString();
            }

            return string.Empty;
        }

        string QueryWhereClause() =>
            string.IsNullOrWhiteSpace(queryFilter.QueryText)
                ? ""
                : timestampWhereClause.Length > 0
                    ? $"AND ({QueryEngine.Translate(queryFilter.QueryText, CreateCodeGenerator())})"
                    : $"WHERE ({QueryEngine.Translate(queryFilter.QueryText, CreateCodeGenerator())})";

        string FiltersWhereClause() =>
            string.IsNullOrWhiteSpace(queryFilter.LevelFilter) && (!queryFilter.ExceptionsOnly)
                ? ""
                : timestampWhereClause.Length > 0 || queryWhereClause.Length > 0
                    ? $" AND ({BuildFilterWhereClause(queryFilter.LevelFilter, queryFilter.ExceptionsOnly)})"
                    : $"WHERE ({BuildFilterWhereClause(queryFilter.LevelFilter, queryFilter.ExceptionsOnly)})";

        CodeGenerator CreateCodeGenerator()
            => (CodeGenerator)Activator.CreateInstance(Settings.CodeGeneratorType)!;
    }

    public async Task CleanupAsync()
    {
        using var conn = GetConnectionCore();
        await conn.OpenAsync();

        const string Sql = "DELETE FROM {0} WHERE timestamp < '{1:yyyy-MM-dd HH:mm:ss.fff}'{2}";

        foreach (var rp in Settings.RetentionPolicies)
        {
            var logsFilter = "";
            var levels = rp.LogLevels.ToArray();

            if (levels.Length > 0)
            {
                var sb = new StringBuilder(" AND level in (");
                for (var i = 0; i < levels.Length; i++)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "'{0}'", levels[i]);
                    if (i < levels.Length - 1)
                    {
                        sb.Append(',');
                    }
                }
                sb.Append(')');
                logsFilter = sb.ToString();
            }

            var sql = string.Format(CultureInfo.InvariantCulture, Sql, LogTableName, DateTime.UtcNow.AddMinutes(-rp.Age.TotalMinutes), logsFilter);
            await conn.ExecuteAsync(sql);
        }
    }

    private static string BuildFilterWhereClause(string? levelFilter, bool exceptionsOnly)
    {
        var sp = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(levelFilter))
        {
            sp.AppendFormat(CultureInfo.InvariantCulture, "level = '{0}'", levelFilter);
        }

        if (exceptionsOnly && sp.Length > 0)
        {
            sp.Append(" AND ");
        }

        if (exceptionsOnly)
        {
            sp.Append("exception is not null");
        }

        return sp.ToString();
    }

    private string BuildDateFilter(LogQueryFilter queryFilter)
    {
        if (queryFilter.DateFilter != null)
        {
            return queryFilter.DateFilter switch
            {
                "5m" => $"timestamp >= {GetDateTimeOffsetSql(-5, "minute")}",
                "15m" => $"timestamp >= {GetDateTimeOffsetSql(-15, "minute")}",
                "1h" => $"timestamp >= {GetDateTimeOffsetSql(-1, "hour")}",
                "6h" => $"timestamp >= {GetDateTimeOffsetSql(-6, "hour")}",
                "12h" => $"timestamp >= {GetDateTimeOffsetSql(-12, "hour")}",
                "24h" => $"timestamp >= {GetDateTimeOffsetSql(-24, "hour")}",
                "2d" => $"timestamp >= {GetDateTimeOffsetSql(-2, "day")}",
                "5d" => $"timestamp >= {GetDateTimeOffsetSql(-5, "day")}",
                _ => "",
            };
        }
        else if (queryFilter.DateRangeFilter != null)
        {
            return $"timestamp >= '{queryFilter.DateRangeFilter[0]:yyyy-MM-dd HH:mm:ss.fff}' and timestamp < '{queryFilter.DateRangeFilter[1]:yyyy-MM-dd HH:mm:ss.fff}'";
        }

        return "";
    }

    private static async Task<string> InsertLogEntryAsync(DbCommand cmd, LogEvent log)
    {
        var id = Guid.NewGuid().ToString();

        cmd.Parameters["@id"].Value = id;
        cmd.Parameters["@message"].Value = log.MessageTemplate.Render(log.Properties);
        cmd.Parameters["@messageTemplate"].Value = log.MessageTemplate.Text;
        cmd.Parameters["@level"].Value = log.Level.ToString();
        cmd.Parameters["@timestamp"].Value = log.Timestamp.ToUniversalTime().DateTime;
        cmd.Parameters["@exception"].Value = log.Exception?.Demystify().ToString() ?? (object)DBNull.Value;

        await cmd.ExecuteNonQueryAsync();
        return id;
    }

    private static async Task InsertLogEntryPropertyAsync(DbCommand cmd, string logId, KeyValuePair<string, LogEventPropertyValue> property)
    {
        cmd.Parameters["@logId"].Value = logId;
        cmd.Parameters["@name"].Value = property.Key;
        cmd.Parameters["@value"].Value = StripStringQuotes(property.Value.ToString());
        await cmd.ExecuteNonQueryAsync();
    }

    private DbCommand CreateLogEntryInsertCommand(DbConnection conn, DbTransaction tran)
    {
        const string Sql = "INSERT INTO {0} (id, message, messageTemplate, level, timestamp, exception)" +
            "VALUES (@id, @message, @messageTemplate, @level, @timestamp, @exception);";

        var cmd = conn.CreateCommand();
        cmd.CommandText = string.Format(CultureInfo.InvariantCulture, Sql, LogTableName);
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = tran;

        cmd.AddParameterWithType("@id", DbType.String);
        cmd.AddParameterWithType("@message", DbType.String);
        cmd.AddParameterWithType("@messageTemplate", DbType.String);
        cmd.AddParameterWithType("@level", DbType.String);
        cmd.AddParameterWithType("@timestamp", DbType.DateTime2);
        cmd.AddParameterWithType("@exception", DbType.String);

        return cmd;
    }

    private DbCommand CreateLogEntryPropertyInsertCommand(DbConnection conn, DbTransaction tran)
    {
        const string Sql = "INSERT INTO {0} (logId, name, value)" +
            "VALUES (@logId, @name, @value);";

        var cmd = conn.CreateCommand();
        cmd.CommandText = string.Format(CultureInfo.InvariantCulture, Sql, LogPropertyTableName);
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = tran;

        cmd.AddParameterWithType("@logId", DbType.String);
        cmd.AddParameterWithType("@name", DbType.String);
        cmd.AddParameterWithType("@value", DbType.String);

        return cmd;
    }

    private static string StripStringQuotes(string value)
        => value[0] == '"' && value[^1] == '"'
            ? value[1..^1]
            : value;
}
