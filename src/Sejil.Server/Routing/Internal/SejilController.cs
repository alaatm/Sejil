using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sejil.Configuration.Internal;
using Sejil.Data.Internal;
using Sejil.Models.Internal;

namespace Sejil.Routing.Internal
{
    public class SejilController : ISejilController
    {
        private static JsonSerializerSettings _camelCaseSerializerSetting = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        private static string _logsHtml = ResourceHelper.GetEmbeddedResource("Sejil.index.html");

        private readonly ISejilRepository _repository;
        private readonly SejilSettings _settings;

        public SejilController(ISejilRepository repository, SejilSettings settings)
        {
            _repository = repository;
            _settings = settings;
        }

        public async Task Index(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(_logsHtml);
        }

        public async Task GetEvents(HttpContext context)
        {
            var query = await GetRequestBodyAsync(context.Request);
            Int32.TryParse(context.Request.Query["page"].FirstOrDefault(), out var page);
            DateTime.TryParse(context.Request.Query["startingTs"].FirstOrDefault(), out var startingTs);

            var events = await _repository.GetPageAsync(page, startingTs, query);

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(events, _camelCaseSerializerSetting));
        }

        public async Task SaveQuery(HttpContext context)
        {
            var logQuery = JsonConvert.DeserializeObject<LogQuery>(await GetRequestBodyAsync(context.Request));
            if (await _repository.SaveQuery(logQuery))
            {
                context.Response.StatusCode = StatusCodes.Status201Created;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }

        public async Task GetQueries(HttpContext context)
        {
            var logQueryList = await _repository.GetSavedQueries();
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(logQueryList, _camelCaseSerializerSetting));
        }

        public async Task SetMinimumLogLevel(HttpContext context)
        {
            var minLogLevel = await GetRequestBodyAsync(context.Request);
            if (_settings.TrySetMinimumLogLevel(minLogLevel))
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid log level.");
            }
        }

        private static async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            if (request.ContentLength > 0)
            {
                var buffer = new byte[(int)request.ContentLength];
                await request.Body.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                return Encoding.UTF8.GetString(buffer);
            }

            return null;
        }
    }
}