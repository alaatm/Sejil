using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Sejil.Routing.Internal
{
    public interface ISejilController
    {
        Task GetIndexAsync(HttpContext context);
        Task GetEventsAsync(HttpContext context);
        Task SaveQueryAsync(HttpContext context);
        Task GetQueriesAsync(HttpContext context);
        Task SetMinimumLogLevelAsync(HttpContext context);
    }
}