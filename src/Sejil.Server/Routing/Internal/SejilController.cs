// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Authentication;
#endif
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
        private HttpContext _context { get; set; }

        public SejilController(IHttpContextAccessor contextAccessor, ISejilRepository repository, ISejilSettings settings)
        {
            _context = contextAccessor.HttpContext;
            _repository = repository;
            _settings = settings;
        }

        public async Task GetIndexAsync()
        {
#if NETSTANDARD2_0
            if (!string.IsNullOrWhiteSpace(_settings.AuthenticationScheme) && !_context.User.Identity.IsAuthenticated)
            {
                await _context.ChallengeAsync(_settings.AuthenticationScheme);
            }
            else
            {
                _context.Response.ContentType = "text/html";
                await _context.Response.WriteAsync(_settings.SejilAppHtml);
            }
#else
            _context.Response.ContentType = "text/html";
            await _context.Response.WriteAsync(_settings.SejilAppHtml);
#endif
        }

        public async Task GetEventsAsync(int page, DateTime? startingTs, LogQueryFilter queryFilter)
        {
            var events = await _repository.GetEventsPageAsync(page == 0 ? 1 : page, startingTs, queryFilter);

            _context.Response.ContentType = "application/json";
            await _context.Response.WriteAsync(JsonConvert.SerializeObject(events, _camelCaseSerializerSetting));
        }

        public async Task SaveQueryAsync(LogQuery logQuery)
        {
            _context.Response.StatusCode = await _repository.SaveQueryAsync(logQuery)
                ? StatusCodes.Status201Created
                : StatusCodes.Status500InternalServerError;
        }

        public async Task GetQueriesAsync()
        {
            var logQueryList = await _repository.GetSavedQueriesAsync();
            _context.Response.ContentType = "application/json";
            await _context.Response.WriteAsync(JsonConvert.SerializeObject(logQueryList, _camelCaseSerializerSetting));
        }

        public async Task GetMinimumLogLevelAsync()
        {
            var response = new
            {
                MinimumLogLevel = _settings.LoggingLevelSwitch.MinimumLevel.ToString()
            };
            _context.Response.ContentType = "application/json";
            await _context.Response.WriteAsync(JsonConvert.SerializeObject(response, _camelCaseSerializerSetting));
        }

        public async Task GetUserNameAsync()
        {
            var response = new
            {
#if NETSTANDARD2_0
                UserName = !string.IsNullOrWhiteSpace(_settings.AuthenticationScheme) && _context.User.Identity.IsAuthenticated
                            ? _context.User.Identity.Name
                            : _context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
#else
                UserName = ""
#endif
            };

            _context.Response.ContentType = "application/json";
            await _context.Response.WriteAsync(JsonConvert.SerializeObject(response, _camelCaseSerializerSetting));
        }

        public void SetMinimumLogLevel(string minLogLevel)
        {
            _context.Response.StatusCode = _settings.TrySetMinimumLogLevel(minLogLevel)
                ? StatusCodes.Status200OK
                : StatusCodes.Status400BadRequest;
        }

        public async Task DeleteQueryAsync(string queryName)
        {
            await _repository.DeleteQueryAsync(queryName);
        }
    }
}