// Copyright (C) 2021 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Sejil.Data.Query;

namespace Sejil.Sqlite.Data.Query;

internal sealed class SqliteCodeGenerator : CodeGenerator
{
    protected override string LogPropertyTableName { get; } = "log_property";
    protected override string NumericCastSql { get; } = "CAST(value AS NUMERIC)";
    protected override string PropertyFilterNegateSql { get; } = "SUM(name = '|PNAME|' AND |VALCOL| |OP| |PVAL|) = 0";
    protected override string PropertyFilterSql { get; } = "SUM(name = '|PNAME|' AND |VALCOL| |OP| |PVAL|) > 0";
}
