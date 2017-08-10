// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

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
        private static JsonSerializerSettings _camelCaseSerializerSetting = 
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

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
            await context.Response.WriteAsync(_settings.SejilAppHtml);
        }

        public async Task GetEventsAsync(HttpContext context, int page, DateTime? startingTs, LogQueryFilter queryFilter)
        {
            var events = await _repository.GetEventsPageAsync(page == 0 ? 1 : page, startingTs, queryFilter);

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(events, _camelCaseSerializerSetting));
        }

        public async Task SaveQueryAsync(HttpContext context, LogQuery logQuery)
        {
            context.Response.StatusCode = await _repository.SaveQueryAsync(logQuery)
                ? StatusCodes.Status201Created
                : StatusCodes.Status500InternalServerError;
        }

        public async Task GetQueriesAsync(HttpContext context)
        {
            var logQueryList = await _repository.GetSavedQueriesAsync();
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(logQueryList, _camelCaseSerializerSetting));
        }

        public void SetMinimumLogLevel(HttpContext context, string minLogLevel)
        {
            context.Response.StatusCode = _settings.TrySetMinimumLogLevel(minLogLevel)
                ? StatusCodes.Status200OK
                : StatusCodes.Status400BadRequest;
        }
    }
}