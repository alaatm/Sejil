using System.Data.Common;
using Microsoft.Data.SqlClient;
using Sejil.Configuration.Internal;
using Sejil.Data.Internal;

namespace Sejil.SqlServer.Data.Internal;

internal sealed class SqlServerSejilRepository : SejilRepository
{
    private readonly string _connectionString;

    public SqlServerSejilRepository(SejilSettings settings, string connectionString)
        : base(settings) => _connectionString = connectionString;

    protected override DbConnection GetConnection()
        => new SqlConnection(_connectionString);

    protected override string GetCreateDatabaseSqlResourceName()
        => "Sejil.SqlServer.db.sql";

    protected override string GetPaginSql(int offset, int take)
        => $"OFFSET {offset} ROWS FETCH NEXT {take} ROWS ONLY";

    protected override string GetDateTimeOffsetSql(int value, string unit)
        => $"DATEADD({unit}, {value}, GETUTCDATE())";
}
