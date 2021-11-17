// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Sejil.Configuration.Internal;
using Sejil.Data.Internal;
using Sejil.Data.Query.Internal;
using Sejil.Models.Internal;
using Sejil.Routing.Internal;
using Serilog.Core;
using Serilog.Events;

namespace Sejil.Test.Routing;

public class SejilControllerTests
{
    private static readonly JsonSerializerOptions _camelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [Fact]
    public async Task GetIndexAsync_writes_app_html_to_response_stream()
    {
        // Arrange
        var appHtml = "<html>...</html>";

        var settingsMoq = new Mock<ISejilSettings>();
        settingsMoq.SetupGet(p => p.SejilAppHtml).Returns(appHtml).Verifiable();
        var context = CreateContext();

        var controller = CreateController(context, Mock.Of<ISejilRepository>(), settingsMoq.Object);

        // Act
        await controller.GetIndexAsync();

        // Assert
        settingsMoq.VerifyAll();
        Assert.Equal("text/html", context.Response.ContentType);
        var data = Encoding.UTF8.GetBytes(appHtml);
        AssertResponseContent(appHtml, context.Response);
    }

    [Fact]
    public async Task GetEventsAsync_retreives_paged_events_from_repository()
    {
        // Arrange
        var page = 1;
        var startingTimestamp = DateTime.Now;

        var repositoryMoq = new Mock<ISejilRepository>();
        var controller = CreateController(CreateContext(), repositoryMoq.Object, Mock.Of<ISejilSettings>());

        // Act
        await controller.GetEventsAsync(page, startingTimestamp, new LogQueryFilter());

        // Assert
        repositoryMoq.Verify(p => p.GetEventsPageAsync(page, startingTimestamp, It.IsAny<LogQueryFilter>()), Times.Once);
    }

    [Fact]
    public async Task GetEventsAsync_sets_page_to_1_when_zero_then_retreives_paged_events_from_repository()
    {
        // Arrange
        var page = 0;
        var startingTimestamp = DateTime.Now;

        var repositoryMoq = new Mock<ISejilRepository>();
        var controller = CreateController(CreateContext(), repositoryMoq.Object, Mock.Of<ISejilSettings>());

        // Act
        await controller.GetEventsAsync(page, startingTimestamp, new LogQueryFilter());

        // Assert
        repositoryMoq.Verify(p => p.GetEventsPageAsync(page + 1, startingTimestamp, It.IsAny<LogQueryFilter>()), Times.Once);
    }

    [Fact]
    public async Task GetEventsAsync_passes_query_filter_to_repository()
    {
        // Arrange
        var qf = new LogQueryFilter
        {
            QueryText = "p=v",
            DateFilter = "5m",
            DateRangeFilter = new List<DateTime>() { DateTime.Now, DateTime.Now }
        };
        var repositoryMoq = new Mock<ISejilRepository>();
        var controller = CreateController(CreateContext(), repositoryMoq.Object, Mock.Of<ISejilSettings>());

        // Act
        await controller.GetEventsAsync(1, null, qf);

        // Assert
        repositoryMoq.Verify(p => p.GetEventsPageAsync(1, null, It.Is<LogQueryFilter>(f => f == qf)), Times.Once);
    }

    [Fact]
    public async Task GetEventsAsync_writes_events_json_to_response_stream()
    {
        // Arrange
        var (logEntries, logEntriesJson) = GetTestLogEntries();
        var repositoryMoq = new Mock<ISejilRepository>();
        repositoryMoq.Setup(p => p.GetEventsPageAsync(1, null, null)).ReturnsAsync(logEntries);
        var context = CreateContext();
        var controller = CreateController(context, repositoryMoq.Object, Mock.Of<ISejilSettings>());

        // Act
        await controller.GetEventsAsync(1, null, null);

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
        AssertResponseContent(logEntriesJson, context.Response);
    }

    [Fact]
    public async Task GetEventsAsync_returns_404_when_GetEventsPageAsync_throws_QueryEngineException()
    {
        // Arrange
        var ex = new QueryEngineException("malformed query.");
        var repositoryMoq = new Mock<ISejilRepository>();
        repositoryMoq.Setup(p => p.GetEventsPageAsync(1, null, null)).ThrowsAsync(ex);
        var context = CreateContext();
        var controller = CreateController(context, repositoryMoq.Object, Mock.Of<ISejilSettings>());

        // Act
        await controller.GetEventsAsync(1, null, null);

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        AssertResponseContent(@"{""error"":""malformed query.""}", context.Response);
    }

    [Fact]
    public async Task SaveQueryAsync_calls_repository_SaveQueryAsync()
    {
        // Arrange
        var logQuery = new LogQuery
        {
            Name = "test",
            Query = "query"
        };
        var repositoryMoq = new Mock<ISejilRepository>();
        var controller = CreateController(CreateContext(), repositoryMoq.Object, Mock.Of<ISejilSettings>());

        // Act
        await controller.SaveQueryAsync(logQuery);

        // Assert
        repositoryMoq.Verify(p => p.SaveQueryAsync(logQuery), Times.Once);
    }

    [Theory]
    [InlineData(true, StatusCodes.Status201Created)]
    [InlineData(false, StatusCodes.Status500InternalServerError)]
    public async Task SaveQueryAsync_sets_status_code_based_on_save_operation_success(bool saveOperationResult, int expectedStatusCode)
    {
        // Arrange
        var repositoryMoq = new Mock<ISejilRepository>();
        repositoryMoq.Setup(p => p.SaveQueryAsync(It.IsAny<LogQuery>())).ReturnsAsync(saveOperationResult);
        var context = CreateContext();
        var controller = CreateController(context, repositoryMoq.Object, Mock.Of<ISejilSettings>());

        // Act
        await controller.SaveQueryAsync(new LogQuery());

        // Assert
        Assert.Equal(expectedStatusCode, context.Response.StatusCode);
    }

    [Fact]
    public async Task GetQueriesAsync_retreives_saved_queries_from_repository()
    {
        // Arrange
        var repositoryMoq = new Mock<ISejilRepository>();
        var controller = CreateController(CreateContext(), repositoryMoq.Object, Mock.Of<ISejilSettings>());

        // Act
        await controller.GetQueriesAsync();

        // Assert
        repositoryMoq.Verify(p => p.GetSavedQueriesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetQueriesAsync_writes_queries_json_to_response_stream()
    {
        // Arrange
        var (logQueries, logQueriesJson) = GetTestLogQueries();
        var repositoryMoq = new Mock<ISejilRepository>();
        repositoryMoq.Setup(p => p.GetSavedQueriesAsync()).ReturnsAsync(logQueries);
        var context = CreateContext();
        var controller = CreateController(context, repositoryMoq.Object, Mock.Of<ISejilSettings>());

        // Act
        await controller.GetQueriesAsync();

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
        AssertResponseContent(logQueriesJson, context.Response);
    }

    [Fact]
    public async Task GetMinimumLogLevelAsync_retreives_minimumLogLevel_from_settings()
    {
        // Arrange
        var levelSwitch = new LoggingLevelSwitch(LogEventLevel.Debug);
        var settingsMoq = new Mock<ISejilSettings>();
        settingsMoq.SetupGet(p => p.LoggingLevelSwitch).Returns(levelSwitch);
        var controller = CreateController(CreateContext(), Mock.Of<ISejilRepository>(), settingsMoq.Object);

        // Act
        await controller.GetMinimumLogLevelAsync();

        // Assert
        settingsMoq.VerifyGet(p => p.LoggingLevelSwitch, Times.Once);
    }

    [Fact]
    public async Task GetMinimumLogLevelAsync_writes_response_json_to_response_stream()
    {
        // Arrange
        var levelSwitch = new LoggingLevelSwitch(LogEventLevel.Debug);
        var settingsMoq = new Mock<ISejilSettings>();
        settingsMoq.SetupGet(p => p.LoggingLevelSwitch).Returns(levelSwitch);

        var responseJson = JsonSerializer.Serialize(new
        {
            MinimumLogLevel = levelSwitch.MinimumLevel.ToString()
        }, _camelCaseJson);

        var context = CreateContext();
        var controller = CreateController(context, Mock.Of<ISejilRepository>(), settingsMoq.Object);

        // Act
        await controller.GetMinimumLogLevelAsync();

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
        AssertResponseContent(responseJson, context.Response);
    }

    [Fact]
    public void SetMinimumLogLevel_calls_settings_TrySetMinimumLogLevel()
    {
        // Arrange
        var logLevel = "info";
        var settingsMoq = new Mock<ISejilSettings>();
        var controller = CreateController(CreateContext(), Mock.Of<ISejilRepository>(), settingsMoq.Object);

        // Act
        controller.SetMinimumLogLevel(logLevel);

        // Assert
        settingsMoq.Verify(p => p.TrySetMinimumLogLevel(logLevel), Times.Once);
    }

    [Theory]
    [InlineData(true, StatusCodes.Status200OK)]
    [InlineData(false, StatusCodes.Status400BadRequest)]
    public void SetMinimumLogLevel_sets_status_code_based_on_save_operation_success(bool saveOperationResult, int expectedStatusCode)
    {
        // Arrange
        var settingsMoq = new Mock<ISejilSettings>();
        settingsMoq.Setup(p => p.TrySetMinimumLogLevel(It.IsAny<string>())).Returns(saveOperationResult);
        var context = CreateContext();
        var controller = CreateController(context, Mock.Of<ISejilRepository>(), settingsMoq.Object);

        // Act
        controller.SetMinimumLogLevel("info");

        // Assert
        Assert.Equal(expectedStatusCode, context.Response.StatusCode);
    }

    [Fact]
    public async Task DeleteQueryAsync_calls_repository_DeleteQueryAsync()
    {
        // Arrange
        var queryName = "query";
        var repositoryMoq = new Mock<ISejilRepository>();
        var controller = CreateController(CreateContext(), repositoryMoq.Object, Mock.Of<ISejilSettings>());

        // Act
        await controller.DeleteQueryAsync(queryName);

        // Assert
        repositoryMoq.Verify(p => p.DeleteQueryAsync(queryName), Times.Once);
    }

    [Fact]
    public async Task GetTitleAsync_writes_configured_title_to_response_stream()
    {
        // Arrange
        var title = "Custom Title";
        var settingsMoq = new Mock<ISejilSettings>();
        settingsMoq.SetupGet(p => p.Title).Returns(title);
        var context = CreateContext();
        var controller = CreateController(context, Mock.Of<ISejilRepository>(), settingsMoq.Object);
        var responseJson = JsonSerializer.Serialize(new
        {
            Title = title
        }, _camelCaseJson);

        // Act
        await controller.GetTitleAsync();

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
        AssertResponseContent(responseJson, context.Response);
    }

    private static ISejilController CreateController(HttpContext context, ISejilRepository repository, ISejilSettings settings)
        => new SejilController(new HttpContextAccessor { HttpContext = context }, repository, settings);

    private static (IEnumerable<LogEntry> logEntries, string logEntriesJson) GetTestLogEntries()
    {
        var events = new List<LogEntry>()
            {
                new LogEntry
                {
                    Id = "001",
                    Message = "message",
                    MessageTemplate = "message template",
                    Properties = new List<LogEntryProperty>
                    {
                        new LogEntryProperty
                        {
                            Id = 1,
                            Name = "prop1",
                            Value = "value1"
                        }
                    }
                }
            };

        var json = JsonSerializer.Serialize(events, _camelCaseJson);

        return (events, json);
    }

    private static (IEnumerable<LogQuery> logQueries, string logQueriesJson) GetTestLogQueries()
    {
        var queries = new List<LogQuery>
            {
                new LogQuery
                {
                    Name = "q1",
                    Query = "query1"
                },
                new LogQuery
                {
                    Name = "q2",
                    Query = "query2"
                },
            };

        var json = JsonSerializer.Serialize(queries, _camelCaseJson);

        return (queries, json);
    }

    private static HttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static void AssertResponseContent(string expected, HttpResponse response)
    {
        response.Body.Position = 0;
        Assert.Equal(expected, new StreamReader(response.Body).ReadToEnd());
    }
}
