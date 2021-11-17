// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Text;
using Sejil.Configuration.Internal;
using Sejil.Models.Internal;
using Sejil.Data.Query.Internal;

namespace Sejil.Data.Internal;

public sealed class SejilSqlProvider : ISejilSqlProvider
{
    public SejilSqlProvider(ISejilSettings _) { }

    public string GetSavedQueriesSql()
        => "SELECT * FROM log_query";

    public string InsertLogQuerySql()
        => "INSERT INTO log_query (name, query) VALUES (@name, @query)";

    public string DeleteQuerySql()
        => "DELETE FROM log_query WHERE name = @name";

    public string GetPagedLogEntriesSql(int page, int pageSize, DateTime? startingTimestamp, LogQueryFilter queryFilter)
    {
        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Argument must be greater than zero.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Argument must be greater than zero.");
        }

        var timestampWhereClause = TimestampWhereClause();
        var queryWhereClause = QueryWhereClause();

        return
$@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    {timestampWhereClause}
    {queryWhereClause}{FiltersWhereClause()}
    ORDER BY timestamp DESC
    LIMIT {pageSize} OFFSET {(page - 1) * pageSize}
) l
LEFT JOIN log_property p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name";

        string TimestampWhereClause()
        {
            var hasStartingTimestampConstraint = startingTimestamp.HasValue;
            var hasDateFilter = queryFilter?.DateFilter != null || queryFilter?.DateRangeFilter != null;

            var sql = new StringBuilder();

            if (hasStartingTimestampConstraint || hasDateFilter)
            {
                sql.Append("WHERE (");
            }

            if (hasStartingTimestampConstraint)
            {
                sql.Append($@"timestamp <= '{startingTimestamp.Value:yyyy-MM-dd HH:mm:ss.fff}'");
            }

            if (hasStartingTimestampConstraint && hasDateFilter)
            {
                sql.Append(" AND ");
            }

            if (hasDateFilter)
            {
                sql.Append(BuildDateFilter(queryFilter));
            }

            if (hasStartingTimestampConstraint || hasDateFilter)
            {
                sql.Append(')');
            }

            return sql.ToString();
        }

        string QueryWhereClause() =>
            string.IsNullOrWhiteSpace(queryFilter?.QueryText)
                ? ""
                : timestampWhereClause.Length > 0
                    ? $"AND ({QueryEngine.Translate(queryFilter.QueryText)})"
                    : $"WHERE ({QueryEngine.Translate(queryFilter.QueryText)})";

        string FiltersWhereClause() =>
            string.IsNullOrWhiteSpace(queryFilter?.LevelFilter) && (!queryFilter?.ExceptionsOnly ?? true)
                ? ""
                : timestampWhereClause.Length > 0 || queryWhereClause.Length > 0
                    ? $" AND ({BuildFilterWhereClause(queryFilter.LevelFilter, queryFilter.ExceptionsOnly)})"
                    : $"WHERE ({BuildFilterWhereClause(queryFilter.LevelFilter, queryFilter.ExceptionsOnly)})";
    }

    private static string BuildFilterWhereClause(string levelFilter, bool exceptionsOnly)
    {
        var sp = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(levelFilter))
        {
            sp.AppendFormat("level = '{0}'", levelFilter);
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

    private static string BuildDateFilter(LogQueryFilter queryFilter)
    {
        if (queryFilter.DateFilter != null)
        {
            return queryFilter.DateFilter switch
            {
                "5m" => "timestamp >= datetime('now', '-5 Minute')",
                "15m" => "timestamp >= datetime('now', '-15 Minute')",
                "1h" => "timestamp >= datetime('now', '-1 Hour')",
                "6h" => "timestamp >= datetime('now', '-6 Hour')",
                "12h" => "timestamp >= datetime('now', '-12 Hour')",
                "24h" => "timestamp >= datetime('now', '-24 Hour')",
                "2d" => "timestamp >= datetime('now', '-2 Day')",
                "5d" => "timestamp >= datetime('now', '-5 Day')",
                _ => "",
            };
        }
        else if (queryFilter.DateRangeFilter != null)
        {
            return $"timestamp >= '{queryFilter.DateRangeFilter[0]:yyyy-MM-dd}' and timestamp < '{queryFilter.DateRangeFilter[1]:yyyy-MM-dd}'";
        }

        return "";
    }
}
