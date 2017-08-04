using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sejil.Models.Internal;

namespace Sejil.Routing.Internal
{
    public interface ISejilController
    {
        Task GetIndexAsync(HttpContext context);
        Task GetEventsAsync(HttpContext context, int page, DateTime? startingTs, string query);
        Task SaveQueryAsync(HttpContext context, LogQuery logQuery);
        Task GetQueriesAsync(HttpContext context);
        void SetMinimumLogLevel(HttpContext context, string minLogLevel);
    }
}