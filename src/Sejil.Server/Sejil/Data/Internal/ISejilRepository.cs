// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Sejil.Models.Internal;

namespace Sejil.Data.Internal;

public interface ISejilRepository
{
    Task<bool> SaveQueryAsync(LogQuery logQuery);
    Task<IEnumerable<LogQuery>> GetSavedQueriesAsync();
    Task<IEnumerable<LogEntry>> GetEventsPageAsync(int page, DateTime? startingTimestamp, LogQueryFilter queryFilter);
    Task<bool> DeleteQueryAsync(string queryName);
}
