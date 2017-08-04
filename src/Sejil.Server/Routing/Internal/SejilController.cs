using System;
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
        private readonly ISejilSettings _settings;

        public SejilController(ISejilRepository repository, ISejilSettings settings)
        {
            _repository = repository;
            _settings = settings;
        }

        public async Task GetIndexAsync(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(_logsHtml);
        }

        public async Task GetEventsAsync(HttpContext context, int page, DateTime? startingTs, string query)
        {
            var events = await _repository.GetEventsPageAsync(page == 0 ? 1 : page, startingTs, query);

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(events, _camelCaseSerializerSetting));
        }

        public async Task SaveQueryAsync(HttpContext context, LogQuery logQuery)
        {
            if (await _repository.SaveQueryAsync(logQuery))
            {
                context.Response.StatusCode = StatusCodes.Status201Created;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }

        public async Task GetQueriesAsync(HttpContext context)
        {
            var logQueryList = await _repository.GetSavedQueriesAsync();
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(logQueryList, _camelCaseSerializerSetting));
        }

        public async Task SetMinimumLogLevelAsync(HttpContext context, string minLogLevel)
        {
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
    }
}