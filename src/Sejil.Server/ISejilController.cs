using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Sejil
{
    public interface ISejilController
    {
        Task Index(HttpContext context);
        Task GetEvents(HttpContext context);
        Task SaveQuery(HttpContext context);
        Task GetQueries(HttpContext context);
        Task SetMinimumLogLevel(HttpContext context);    
    }
}