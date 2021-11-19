//using Sejil.Configuration.Internal;
//using Sejil.Data.Internal;

//namespace Sejil.Sqlite.Data.Internal;

//internal sealed class SqliteSejilSqlProvider : SejilSqlProvider
//{
//    public SqliteSejilSqlProvider(SejilSettings settings) : base(settings)
//    {
//    }

//    protected override string GetCreateDatabaseSqlResourceName()
//        => "Sejil.Sqlite.db.sql";

//    protected override string GetPaginSql(int offset, int take)
//        => $"LIMIT {take} OFFSET {offset}";

//    protected override string GetDateTimeOffsetSql(int value, string unit)
//        => $"datetime('now', '{value} {unit}')";
//}
