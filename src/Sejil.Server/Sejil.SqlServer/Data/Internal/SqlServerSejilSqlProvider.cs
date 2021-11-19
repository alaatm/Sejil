//using Sejil.Configuration.Internal;
//using Sejil.Data.Internal;

//namespace Sejil.SqlServer.Data.Internal;

//internal sealed class SqlServerSejilSqlProvider : SejilSqlProvider
//{
//    public SqlServerSejilSqlProvider(SejilSettings settings) : base(settings)
//    {
//    }

//    protected override string GetCreateDatabaseSqlResourceName()
//        => "Sejil.SqlServer.db.sql";

//    protected override string GetPaginSql(int offset, int take)
//        => $"OFFSET {offset} ROWS FETCH NEXT {take} ROWS ONLY";

//    protected override string GetDateTimeOffsetSql(int value, string unit)
//        => $"DATEADD({unit}, {value}, GETUTCDATE())";
//}
