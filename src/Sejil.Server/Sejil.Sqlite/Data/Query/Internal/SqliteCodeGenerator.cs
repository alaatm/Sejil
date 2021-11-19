using Sejil.Data.Query.Internal;

namespace Sejil.Sqlite.Data.Query.Internal;

internal sealed class SqliteCodeGenerator : CodeGenerator
{
    protected override string NumericCastSql { get; } = "CAST(value AS NUMERIC)";
    protected override string PropertyFilterNegateSql { get; } = "SUM(name = '|PNAME|' AND |VALCOL| |OP| |PVAL|) = 0";
    protected override string PropertyFilterSql { get; } = "SUM(name = '|PNAME|' AND |VALCOL| |OP| |PVAL|) > 0";
}
