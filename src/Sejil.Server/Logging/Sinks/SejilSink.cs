using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using Sejil.Configuration.Internal;

namespace Sejil.Logging.Sinks
{
    internal class SejilSink : PeriodicBatchingSink
    {
        private static int _defaultBatchSizeLimit = 50;
        private static TimeSpan _defaultBatchEmitPeriod = TimeSpan.FromSeconds(5);

        private readonly string _connectionString;
        private readonly string _uri;

        public SejilSink(SejilSettings settings) : base(_defaultBatchSizeLimit, _defaultBatchEmitPeriod)
        {
            _connectionString = $"DataSource={settings.SqliteDbPath}";
            _uri = settings.Uri;

            InitializeDatabase();
        }

        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            try
            {
                using (var conn = new SqliteConnection(_connectionString))
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    using (var tran = conn.BeginTransaction())
                    {
                        using (var cmdLogEntry = CreateLogEntryInsertCommand(conn, tran))
                        using (var cmdLogEntryProperty = CreateLogEntryPropertyInsertCommand(conn, tran))
                        {
                            foreach (var logEvent in events)
                            {
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
            }
            catch (Exception e)
            {
                SelfLog.WriteLine(e.Message);
            }
        }

        private async Task<string> InsertLogEntryAsync(SqliteCommand cmd, LogEvent log)
        {
            var id = Guid.NewGuid().ToString();

            cmd.Parameters["@id"].Value = id;
            cmd.Parameters["@message"].Value = log.MessageTemplate.Render(log.Properties);
            cmd.Parameters["@message_template"].Value = log.MessageTemplate.Text;
            cmd.Parameters["@level"].Value = log.Level.ToString();
            cmd.Parameters["@timestamp"].Value = log.Timestamp.ToUniversalTime();
            cmd.Parameters["@exception"].Value = log.Exception?.ToString() ?? (object)DBNull.Value;

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            return id;
        }

        private async Task InsertLogEntryPropertyAsync(SqliteCommand cmd, string logId, KeyValuePair<string, LogEventPropertyValue> property)
        {
            cmd.Parameters["@log_id"].Value = logId;
            cmd.Parameters["@name"].Value = property.Key;
            cmd.Parameters["@value"].Value = StripStringQuotes(property.Value?.ToString()) ?? (object)DBNull.Value;
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        private SqliteCommand CreateLogEntryInsertCommand(SqliteConnection conn, SqliteTransaction tran)
        {
            var sql = "INSERT INTO log (id, message, message_template, level, timestamp, exception)" +
                "VALUES (@id, @message, @message_template, @level, @timestamp, @exception);";

            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = tran;

            cmd.Parameters.Add(new SqliteParameter("@id", DbType.String));
            cmd.Parameters.Add(new SqliteParameter("@message", DbType.String));
            cmd.Parameters.Add(new SqliteParameter("@message_template", DbType.String));
            cmd.Parameters.Add(new SqliteParameter("@level", DbType.String));
            cmd.Parameters.Add(new SqliteParameter("@timestamp", DbType.DateTime2));
            cmd.Parameters.Add(new SqliteParameter("@exception", DbType.String));

            return cmd;
        }

        private SqliteCommand CreateLogEntryPropertyInsertCommand(SqliteConnection conn, SqliteTransaction tran)
        {
            var sql = "INSERT INTO log_property (log_id, name, value)" +
                "VALUES (@log_id, @name, @value);";

            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = tran;

            cmd.Parameters.Add(new SqliteParameter("@log_id", DbType.String));
            cmd.Parameters.Add(new SqliteParameter("@name", DbType.String));
            cmd.Parameters.Add(new SqliteParameter("@value", DbType.String));

            return cmd;
        }

        private void InitializeDatabase()
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                var sql = ResourceHelper.GetEmbeddedResource("Sejil.db.sql");
                var cmd = new SqliteCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }

        private string StripStringQuotes(string value)
            => (value?.Length > 0 && value[0] == '"' && value[value.Length - 1] == '"')
                ? value.Substring(1, value.Length - 2)
                : value;
    }
}
