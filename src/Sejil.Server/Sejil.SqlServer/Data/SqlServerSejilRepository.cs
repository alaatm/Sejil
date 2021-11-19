// Copyright (C) 2021 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Data.Common;
using Microsoft.Data.SqlClient;
using Sejil.Configuration;
using Sejil.Data;

namespace Sejil.SqlServer.Data;

internal sealed class SqlServerSejilRepository : SejilRepository
{
    public SqlServerSejilRepository(ISejilSettings settings, string connectionString)
        : base(settings, connectionString)
    {
    }

    protected override void InitializeDatabase()
    {
        var initSql = ResourceHelper.GetEmbeddedResource(typeof(SqlServerSejilRepository).Assembly, "Sejil.SqlServer.db.sql");

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = initSql;
        cmd.ExecuteNonQuery();
    }

    protected override DbConnection GetConnection()
        => new SqlConnection(ConnectionString);

    protected override string GetPaginSql(int offset, int take)
        => $"OFFSET {offset} ROWS FETCH NEXT {take} ROWS ONLY";

    protected override string GetDateTimeOffsetSql(int value, string unit)
        => $"DATEADD({unit}, {value}, GETUTCDATE())";
}
