// Copyright (C) 2021 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Data.Common;
using Microsoft.Data.Sqlite;
using Sejil.Configuration;
using Sejil.Data;

namespace Sejil.Sqlite.Data;

internal sealed class SqliteSejilRepository : SejilRepository
{
    public SqliteSejilRepository(ISejilSettings settings, string connectionString)
        : base(settings, connectionString)
    {
    }

    protected override void InitializeDatabase()
    {
        var initSql = ResourceHelper.GetEmbeddedResource(typeof(SqliteSejilRepository).Assembly, "Sejil.Sqlite.db.sql");

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = initSql;
        cmd.ExecuteNonQuery();
    }

    protected override DbConnection GetConnection()
        => new SqliteConnection(ConnectionString);

    protected override string GetPaginSql(int offset, int take)
        => $"LIMIT {take} OFFSET {offset}";

    protected override string GetDateTimeOffsetSql(int value, string unit)
        => $"datetime('now', '{value} {unit}')";
}
