using System;

namespace Sejil.Data.Internal
{
    public interface ISejilSqlProvider
    {
        string InsertLogQuerySql();
        string GetSavedQueriesSql();
        string GetPagedLogEntriesSql(int page, int pageSize, DateTime? startingTimestamp, string query);
    }
}