// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Sejil.Models.Internal;
using Serilog.Events;

namespace Sejil.Data.Internal;

internal interface ISejilRepository
{
    Task<IEnumerable<LogQuery>> GetSavedQueriesAsync();
    Task<bool> SaveQueryAsync(LogQuery logQuery);
    Task<bool> DeleteQueryAsync(string queryName);
    Task InsertEventsAsync(IEnumerable<LogEvent> events);
    Task<IEnumerable<LogEntry>> GetEventsPageAsync(int page, DateTime? startingTimestamp, LogQueryFilter queryFilter);
}
