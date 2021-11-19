using System.Data.Common;
using Microsoft.Data.Sqlite;
using Sejil.Configuration.Internal;
using Sejil.Data.Internal;

namespace Sejil.Sqlite.Data.Internal;

internal sealed class SqliteSejilRepository : SejilRepository
{
    private readonly string _connectionString;

    public SqliteSejilRepository(SejilSettings settings, string connectionString)
        : base(settings) => _connectionString = connectionString;

    protected override DbConnection GetConnection()
        => new SqliteConnection(_connectionString);

    protected override string GetCreateDatabaseSqlResourceName()
        => "Sejil.Sqlite.db.sql";

    protected override string GetPaginSql(int offset, int take)
        => $"LIMIT {take} OFFSET {offset}";

    protected override string GetDateTimeOffsetSql(int value, string unit)
        => $"datetime('now', '{value} {unit}')";
}
