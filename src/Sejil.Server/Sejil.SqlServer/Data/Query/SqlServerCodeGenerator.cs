// Copyright (C) 2021 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Sejil.Data.Query;

namespace Sejil.SqlServer.Data.Query;

internal sealed class SqlServerCodeGenerator : CodeGenerator
{
    protected override string LogPropertyTableName { get; } = "[sejil].[log_property]";
    protected override string NumericCastSql { get; } = "CAST(value AS NUMERIC)";
    protected override string PropertyFilterNegateSql { get; } = "SUM(CASE WHEN name = '|PNAME|' AND |VALCOL| |OP| |PVAL| THEN 1 ELSE 0 END) = 0";
    protected override string PropertyFilterSql { get; } = "SUM(CASE WHEN name = '|PNAME|' AND |VALCOL| |OP| |PVAL| THEN 1 ELSE 0 END) > 0";
}
