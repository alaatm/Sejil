// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Sejil.Configuration.Internal;
using Sejil.Data.Internal;
using Sejil.Models.Internal;
using System.Text.Json;
using Sejil.Data.Query.Internal;

namespace Sejil.Routing.Internal
{
    public sealed class SejilController : ISejilController
    {
        private readonly ISejilRepository _repository;
        private readonly ISejilSettings _settings;
        private readonly HttpContext _context;

        public SejilController(IHttpContextAccessor contextAccessor, ISejilRepository repository, ISejilSettings settings)
            => (_context, _repository, _settings) = (contextAccessor.HttpContext, repository, settings);

        public async Task GetIndexAsync()
        {
            if (!string.IsNullOrWhiteSpace(_settings.AuthenticationScheme) && !_context.User.Identity.IsAuthenticated)
            {
                await _context.ChallengeAsync(_settings.AuthenticationScheme);
            }
            else
            {
                _context.Response.ContentType = "text/html";
                await _context.Response.WriteAsync(_settings.SejilAppHtml);
            }
        }

        public async Task GetEventsAsync(int page, DateTime? startingTs, LogQueryFilter queryFilter)
        {
            try
            {
                var events = await _repository.GetEventsPageAsync(page == 0 ? 1 : page, startingTs, queryFilter);
                _context.Response.ContentType = "application/json";
                await _context.Response.WriteAsync(JsonSerializer.Serialize(events, ApplicationBuilderExtensions.CamelCaseJson));
            }
            catch (QueryEngineException ex)
            {
                _context.Response.ContentType = "application/json";
                _context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await _context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }, ApplicationBuilderExtensions.CamelCaseJson));
            }
        }

        public async Task SaveQueryAsync(LogQuery logQuery) =>
            _context.Response.StatusCode = await _repository.SaveQueryAsync(logQuery)
                ? StatusCodes.Status201Created
                : StatusCodes.Status500InternalServerError;

        public async Task GetQueriesAsync()
        {
            var logQueryList = await _repository.GetSavedQueriesAsync();
            _context.Response.ContentType = "application/json";
            await _context.Response.WriteAsync(JsonSerializer.Serialize(logQueryList, ApplicationBuilderExtensions.CamelCaseJson));
        }

        public async Task GetMinimumLogLevelAsync()
        {
            var response = new
            {
                MinimumLogLevel = _settings.LoggingLevelSwitch.MinimumLevel.ToString()
            };
            _context.Response.ContentType = "application/json";
            await _context.Response.WriteAsync(JsonSerializer.Serialize(response, ApplicationBuilderExtensions.CamelCaseJson));
        }

        public async Task GetUserNameAsync()
        {
            var response = new
            {
                UserName = !string.IsNullOrWhiteSpace(_settings.AuthenticationScheme) && _context.User.Identity.IsAuthenticated
                    ? _context.User.Identity.Name
                    : ""
            };

            _context.Response.ContentType = "application/json";
            await _context.Response.WriteAsync(JsonSerializer.Serialize(response, ApplicationBuilderExtensions.CamelCaseJson));
        }

        public void SetMinimumLogLevel(string minLogLevel) =>
            _context.Response.StatusCode = _settings.TrySetMinimumLogLevel(minLogLevel)
                ? StatusCodes.Status200OK
                : StatusCodes.Status400BadRequest;

        public async Task DeleteQueryAsync(string queryName)
            => await _repository.DeleteQueryAsync(queryName);

        public async Task GetTitleAsync()
        {
            var response = new
            {
                _settings.Title
            };
            _context.Response.ContentType = "application/json";
            await _context.Response.WriteAsync(JsonSerializer.Serialize(response, ApplicationBuilderExtensions.CamelCaseJson));
        }
    }
}
