// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Data;
using Microsoft.Data.Sqlite;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using Sejil.Configuration.Internal;
using System.Diagnostics;

namespace Sejil.Logging.Sinks
{
    internal class SejilSink : PeriodicBatchingSink
    {
        private const int DefaultBatchSizeLimit = 50;
        private static readonly TimeSpan _defaultBatchEmitPeriod = TimeSpan.FromSeconds(5);

        private readonly string _connectionString;
        private readonly string _uri;

        public SejilSink(ISejilSettings settings) : base(DefaultBatchSizeLimit, _defaultBatchEmitPeriod)
        {
            _connectionString = $"DataSource={settings.SqliteDbPath}";
            _uri = settings.Url;

            InitializeDatabase();
        }

        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            using (var tran = conn.BeginTransaction())
            {
                using (var cmdLogEntry = CreateLogEntryInsertCommand(conn, tran))
                using (var cmdLogEntryProperty = CreateLogEntryPropertyInsertCommand(conn, tran))
                {
                    foreach (var logEvent in events)
                    {
                        // Do not log events that were generated from browsing Sejil URL.
                        if (logEvent.Properties.Any(p => (p.Key == "RequestPath" || p.Key == "Path") &&
                            p.Value.ToString().Contains(_uri)))
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
                tran.Commit();
            }
            conn.Close();
        }

        private static async Task<string> InsertLogEntryAsync(SqliteCommand cmd, LogEvent log)
        {
            var id = Guid.NewGuid().ToString();

            cmd.Parameters["@id"].Value = id;
            cmd.Parameters["@message"].Value = log.MessageTemplate.Render(log.Properties);
            cmd.Parameters["@messageTemplate"].Value = log.MessageTemplate.Text;
            cmd.Parameters["@level"].Value = log.Level.ToString();
            cmd.Parameters["@timestamp"].Value = log.Timestamp.ToUniversalTime();
            cmd.Parameters["@exception"].Value = log.Exception?.Demystify().ToString() ?? (object)DBNull.Value;

            await cmd.ExecuteNonQueryAsync();
            return id;
        }

        private static async Task InsertLogEntryPropertyAsync(SqliteCommand cmd, string logId, KeyValuePair<string, LogEventPropertyValue> property)
        {
            cmd.Parameters["@logId"].Value = logId;
            cmd.Parameters["@name"].Value = property.Key;
            cmd.Parameters["@value"].Value = StripStringQuotes(property.Value.ToString());
            await cmd.ExecuteNonQueryAsync();
        }

        private static SqliteCommand CreateLogEntryInsertCommand(SqliteConnection conn, SqliteTransaction tran)
        {
            var sql = "INSERT INTO log (id, message, messageTemplate, level, timestamp, exception)" +
                "VALUES (@id, @message, @messageTemplate, @level, @timestamp, @exception);";

            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = tran;

            cmd.Parameters.Add(new SqliteParameter("@id", DbType.String));
            cmd.Parameters.Add(new SqliteParameter("@message", DbType.String));
            cmd.Parameters.Add(new SqliteParameter("@messageTemplate", DbType.String));
            cmd.Parameters.Add(new SqliteParameter("@level", DbType.String));
            cmd.Parameters.Add(new SqliteParameter("@timestamp", DbType.DateTime2));
            cmd.Parameters.Add(new SqliteParameter("@exception", DbType.String));

            return cmd;
        }

        private static SqliteCommand CreateLogEntryPropertyInsertCommand(SqliteConnection conn, SqliteTransaction tran)
        {
            var sql = "INSERT INTO log_property (logId, name, value)" +
                "VALUES (@logId, @name, @value);";

            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = tran;

            cmd.Parameters.Add(new SqliteParameter("@logId", DbType.String));
            cmd.Parameters.Add(new SqliteParameter("@name", DbType.String));
            cmd.Parameters.Add(new SqliteParameter("@value", DbType.String));

            return cmd;
        }

        private void InitializeDatabase()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var sql = ResourceHelper.GetEmbeddedResource("Sejil.db.sql");
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private static string StripStringQuotes(string value)
            => value[0] == '"' && value[value.Length - 1] == '"'
                ? value.Substring(1, value.Length - 2)
                : value;
    }
}
