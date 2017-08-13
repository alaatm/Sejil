// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Sejil.Configuration.Internal;
using Sejil.Models.Internal;

namespace Sejil.Data.Internal
{
    public class SejilRepository : ISejilRepository
    {
        private readonly ISejilSqlProvider _sql;
        private readonly string _connectionString;
        private readonly int _pageSize;

        public SejilRepository(ISejilSqlProvider sql, ISejilSettings settings)
        {
            _sql = sql;
            _connectionString = $"DataSource={settings.SqliteDbPath}";
            _pageSize = settings.PageSize;
        }

        public async Task<bool> SaveQueryAsync(LogQuery logQuery)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = _sql.InsertLogQuerySql();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@name", logQuery.Name);
                    cmd.Parameters.AddWithValue("@query", logQuery.Query);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        public async Task<IEnumerable<LogQuery>> GetSavedQueriesAsync()
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                return conn.Query<LogQuery>(_sql.GetSavedQueriesSql());
            }
        }

        public async Task<IEnumerable<LogEntry>> GetEventsPageAsync(int page, DateTime? startingTimestamp, LogQueryFilter queryFilter)
        {
            var sql = _sql.GetPagedLogEntriesSql(page, _pageSize, startingTimestamp, queryFilter);

            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var lookup = new Dictionary<string, LogEntry>();

                var data = conn.Query<LogEntry, LogEntryProperty, LogEntry>(sql, (l, p) =>
                    {
                        LogEntry logEntry;
                        if (!lookup.TryGetValue(l.Id, out logEntry))
                        {
                            lookup.Add(l.Id, logEntry = l);
                        }

                        if (p != null)
                        {
                            logEntry.Properties.Add(p);
                        }
                        return logEntry;

                    }).ToList();

                return lookup.Values.AsEnumerable();
            }
        }

        public async Task<bool> DeleteQueryAsync(string queryName)
        {
            using (var conn = new SqliteConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = _sql.DeleteQuerySql();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@name", queryName);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }
    }
}