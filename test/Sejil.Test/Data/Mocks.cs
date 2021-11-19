using Microsoft.Data.Sqlite;
using System.Data.Common;
using Sejil.Data.Internal;
using Sejil.Data.Query.Internal;
using Sejil.Configuration.Internal;
using Serilog.Events;

namespace Sejil.Test.Data;

internal static class Mocks
{
    public static SejilSettings GetTestSettings(int pageSize = 100) => new("/sejil", LogEventLevel.Information, pageSize)
    {
        CodeGeneratorType = typeof(CodeGeneratorMoq),
    };
}

internal class SejilRepositoryMoq : SejilRepository
{
    private readonly string _dbName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public SejilRepositoryMoq(SejilSettings settings) : base(settings) { }
    protected override DbConnection GetConnection() => new SqliteConnection($"DataSource={_dbName}");
    protected override string GetPaginSql(int offset, int take) => $"LIMIT {take} OFFSET {offset}";
    protected override string GetDateTimeOffsetSql(int value, string unit) => $"datetime('now', '{value} {unit}')";
    protected override string GetCreateDatabaseSqlResourceName() => "Sejil.db.sql";
}

internal class CodeGeneratorMoq : CodeGenerator
{
    protected override string NumericCastSql { get; } = "CAST(value AS NUMERIC)";
    protected override string PropertyFilterNegateSql { get; } = "SUM(name = '|PNAME|' AND |VALCOL| |OP| |PVAL|) = 0";
    protected override string PropertyFilterSql { get; } = "SUM(name = '|PNAME|' AND |VALCOL| |OP| |PVAL|) > 0";
}
