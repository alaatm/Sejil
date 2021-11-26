using System.Reflection;
using Sejil.Configuration;
using Sejil.Models;
using Sejil.Sqlite.Data;
using Sejil.Sqlite.Data.Query;
using Serilog;
using Serilog.Events;

namespace Sejil.Sqlite.Test;

public class SqliteSejilRepositoryTests
{
    private readonly SqliteSejilRepository _repository;

    public SqliteSejilRepositoryTests()
    {
        var connStr = $"DataSource={Guid.NewGuid()}";
        _repository = new SqliteSejilRepository(new SejilSettings("/sejil", default) { CodeGeneratorType = typeof(SqliteCodeGenerator) }, connStr);
        _repository.InsertEventsAsync(GetTestEvents()).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Can_save_load_delete_queries()
    {
        var queryName = "TestName";

        await _repository.SaveQueryAsync(new LogQuery { Name = queryName, Query = "TestQuery" });
        var savedQuery = Assert.Single(await _repository.GetSavedQueriesAsync());
        Assert.Equal(queryName, savedQuery.Name);
        Assert.Equal("TestQuery", savedQuery.Query);

        await _repository.DeleteQueryAsync(queryName);
        Assert.Empty(await _repository.GetSavedQueriesAsync());
    }

    [Fact]
    public async Task Can_filter_by_level()
    {
        var e = Assert.Single(await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { LevelFilter = "Debug" }));
        Assert.Equal(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 11, 5, 5, 5, DateTimeKind.Local)), e.Timestamp);
        Assert.Equal("Debug", e.Level);
        Assert.Equal("Object is \"{ Id = 5, Name = Test Object }\"", e.Message);
        Assert.Null(e.Exception);
    }

    [Fact]
    public async Task Can_filter_by_date()
        => Assert.Empty(await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { DateFilter = "5m" }));

    [Fact]
    public async Task Can_filter_by_dateRange()
    {
        var start = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 12, 0, 0, DateTimeKind.Local));
        var end = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 12, 10, 0, DateTimeKind.Local));

        var e = Assert.Single(await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { DateRangeFilter = new List<DateTime> { start, end } }));
        Assert.Equal(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 12, 5, 5, 5, DateTimeKind.Local)), e.Timestamp);
        Assert.Equal("Warning", e.Level);
        Assert.Equal("This is a warning with value: null", e.Message);
        Assert.Null(e.Exception);
    }

    [Fact]
    public async Task Can_filters_by_exceptionOnly()
    {
        var e = Assert.Single(await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { ExceptionsOnly = true }));
        Assert.Equal(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 13, 5, 5, 5, DateTimeKind.Local)), e.Timestamp);
        Assert.Equal("Error", e.Level);
        Assert.Equal("This is an exception", e.Message);
        Assert.Equal("System.Exception: Test exception", e.Exception);
    }

    [Fact]
    public async Task Can_filter_by_query()
    {
        var e = Assert.Single(await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { QueryText = "name = 'test name'" }));
        Assert.Equal(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 10, 5, 5, 5, DateTimeKind.Local)), e.Timestamp);
        Assert.Equal("Information", e.Level);
        Assert.Equal("Name is \"Test name\" and Value is \"Test value\"", e.Message);
        Assert.Null(e.Exception);
    }

    [Fact]
    public async Task CleanupAsync_test()
    {
        // Arrange
        var settings = (ISejilSettings)typeof(SqliteSejilRepository)
            .GetProperty("Settings", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(_repository);

        settings
            .AddRetentionPolicy(TimeSpan.FromHours(5), LogEventLevel.Verbose, LogEventLevel.Debug)
            .AddRetentionPolicy(TimeSpan.FromDays(10), LogEventLevel.Information)
            .AddRetentionPolicy(TimeSpan.FromDays(75));

        var now = DateTime.UtcNow;
        await _repository.InsertEventsAsync(new[]
        {
            BuildLogEvent(now.AddHours(-5.1), LogEventLevel.Verbose, null, "Verbose #{Num}", 1),
            BuildLogEvent(now.AddHours(-5.1), LogEventLevel.Debug, null, "Debug #{Num}", 1),
            BuildLogEvent(now, LogEventLevel.Verbose, null, "Verbose #{Num}", 2),
            BuildLogEvent(now, LogEventLevel.Debug, null, "Debug #{Num}", 2),

            BuildLogEvent(now.AddDays(-10.1), LogEventLevel.Information, null, "Information #{Num}", 1),
            BuildLogEvent(now, LogEventLevel.Information, null, "Information #{Num}", 2),

            BuildLogEvent(now.AddDays(-75.1), LogEventLevel.Warning, null, "Warning #{Num}", 1),
            BuildLogEvent(now.AddDays(-75.1), LogEventLevel.Error, null, "Error #{Num}", 1),
            BuildLogEvent(now.AddDays(-75.1), LogEventLevel.Fatal, null, "Fatal #{Num}", 1),
            BuildLogEvent(now, LogEventLevel.Warning, null, "Warning #{Num}", 2),
            BuildLogEvent(now, LogEventLevel.Error, null, "Error #{Num}", 2),
            BuildLogEvent(now, LogEventLevel.Fatal, null, "Fatal #{Num}", 2),
        });

        // Act
        Assert.Equal(6, (await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { QueryText = "Num=1" })).Count());
        await _repository.CleanupAsync();

        // Assert
        Assert.Empty(await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { QueryText = "Num=1" }));
        Assert.Equal(6, (await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { QueryText = "Num=2" })).Count());
    }

    private static IEnumerable<LogEvent> GetTestEvents() => new[]
    {
        BuildLogEvent(new DateTime(2017, 8, 3, 10, 5, 5, 5, DateTimeKind.Local), LogEventLevel.Information, null, "Name is {Name} and Value is {Value}", "Test name", "Test value"),
        BuildLogEvent(new DateTime(2017, 8, 3, 11, 5, 5, 5, DateTimeKind.Local), LogEventLevel.Debug, null, "Object is {Object}", new { Id = 5, Name = "Test Object" }),
        BuildLogEvent(new DateTime(2017, 8, 3, 12, 5, 5, 5, DateTimeKind.Local), LogEventLevel.Warning, null, "This is a warning with value: {Value}", (string)null),
        BuildLogEvent(new DateTime(2017, 8, 3, 13, 5, 5, 5, DateTimeKind.Local), LogEventLevel.Error, new Exception("Test exception"), "This is an exception"),
    };

    public static LogEvent BuildLogEvent(DateTime timestamp, LogEventLevel level, Exception ex, string messageTemplate, params object[] propertyValues)
    {
        var logger = new LoggerConfiguration().CreateLogger();
        logger.BindMessageTemplate(messageTemplate, propertyValues, out var parsedTemplate, out var boundProperties);
        return new LogEvent(timestamp, level, ex, parsedTemplate, boundProperties);
    }
}
