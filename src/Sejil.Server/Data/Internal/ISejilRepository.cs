using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sejil.Models.Internal;

namespace Sejil.Data.Internal
{
    public interface ISejilRepository
    {
        Task<bool> SaveQuery(LogQuery logQuery);        
        Task<IEnumerable<LogQuery>> GetSavedQueries();
        Task<IEnumerable<LogEntry>> GetPageAsync(int page, DateTime startingTimestamp, string query);
    }
}