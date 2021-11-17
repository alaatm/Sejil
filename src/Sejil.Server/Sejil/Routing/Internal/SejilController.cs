// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Sejil.Configuration.Internal;
using Sejil.Data.Internal;
using Sejil.Data.Query.Internal;
using Sejil.Models.Internal;

namespace Sejil.Routing.Internal;

public sealed class SejilController : ISejilController
{
    private readonly ISejilRepository _repository;
    private readonly ISejilSettings _settings;
    private readonly IHttpContextAccessor _contextAcessor;

    public SejilController(IHttpContextAccessor contextAccessor, ISejilRepository repository, ISejilSettings settings)
        => (_contextAcessor, _repository, _settings) = (contextAccessor, repository, settings);

    public async Task GetIndexAsync()
    {
        var context = _contextAcessor.HttpContext!;

        if (!string.IsNullOrWhiteSpace(_settings.AuthenticationScheme) && (!context.User.Identity?.IsAuthenticated ?? false))
        {
            await context.ChallengeAsync(_settings.AuthenticationScheme);
        }
        else
        {
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(_settings.SejilAppHtml);
        }
    }

    public async Task GetEventsAsync(int page, DateTime? startingTs, LogQueryFilter queryFilter)
    {
        var context = _contextAcessor.HttpContext!;

        try
        {
            var events = await _repository.GetEventsPageAsync(page == 0 ? 1 : page, startingTs, queryFilter);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(events, ApplicationBuilderExtensions.CamelCaseJson));
        }
        catch (QueryEngineException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }, ApplicationBuilderExtensions.CamelCaseJson));
        }
    }

    public async Task SaveQueryAsync(LogQuery logQuery) =>
        _contextAcessor.HttpContext!.Response.StatusCode = await _repository.SaveQueryAsync(logQuery)
            ? StatusCodes.Status201Created
            : StatusCodes.Status500InternalServerError;

    public async Task GetQueriesAsync()
    {
        var context = _contextAcessor.HttpContext!;

        var logQueryList = await _repository.GetSavedQueriesAsync();
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(logQueryList, ApplicationBuilderExtensions.CamelCaseJson));
    }

    public async Task GetMinimumLogLevelAsync()
    {
        var context = _contextAcessor.HttpContext!;

        var response = new
        {
            MinimumLogLevel = _settings.LoggingLevelSwitch.MinimumLevel.ToString()
        };
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, ApplicationBuilderExtensions.CamelCaseJson));
    }

    public async Task GetUserNameAsync()
    {
        var context = _contextAcessor.HttpContext!;

        var response = new
        {
            UserName = !string.IsNullOrWhiteSpace(_settings.AuthenticationScheme) && (context.User.Identity?.IsAuthenticated ?? false)
                ? context.User.Identity.Name
                : ""
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, ApplicationBuilderExtensions.CamelCaseJson));
    }

    public void SetMinimumLogLevel(string minLogLevel) =>
        _contextAcessor.HttpContext!.Response.StatusCode = _settings.TrySetMinimumLogLevel(minLogLevel)
            ? StatusCodes.Status200OK
            : StatusCodes.Status400BadRequest;

    public async Task DeleteQueryAsync(string queryName)
        => await _repository.DeleteQueryAsync(queryName);

    public async Task GetTitleAsync()
    {
        var context = _contextAcessor.HttpContext!;

        var response = new
        {
            _settings.Title
        };
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, ApplicationBuilderExtensions.CamelCaseJson));
    }
}
