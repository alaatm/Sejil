// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using Sejil.Models.Internal;

namespace Sejil.Data.Internal
{
    public interface ISejilSqlProvider
    {
        string InsertLogQuerySql();
        string GetSavedQueriesSql();
        string GetPagedLogEntriesSql(int page, int pageSize, DateTime? startingTimestamp, LogQueryFilter queryFilter);
    }
}