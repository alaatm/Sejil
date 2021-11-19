// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Sejil.Models;
using Serilog.Events;
using Serilog.Parsing;

namespace Sejil.Test.Data;

public partial class SejilRepositoryTests
{
    [Fact]
    public async Task SaveQueryAsync_saves_query()
    {
        // Arrange
        var repository = new SejilRepositoryMoq(Mocks.GetTestSettings());

        var logQuery = new LogQuery
        {
            Id = 1,
            Name = "Test",
            Query = "q"
        };

        // Act
        var result = await repository.SaveQueryAsync(logQuery);

        // Assert
        Assert.True(result);
        var savedQueries = await repository.GetSavedQueriesAsync();
        Assert.Single(savedQueries);
        Assert.Equal(1, savedQueries.First().Id);
        Assert.Equal("Test", savedQueries.First().Name);
        Assert.Equal("q", savedQueries.First().Query);
    }

    [Fact]
    public async Task GetSavedQueriesAsync_returns_saved_queries()
    {
        // Arrange
        var repository = new SejilRepositoryMoq(Mocks.GetTestSettings());
        await repository.SaveQueryAsync(new LogQuery { Name = "Test1", Query = "q1" });
        await repository.SaveQueryAsync(new LogQuery { Name = "Test2", Query = "q2" });

        // Act
        var queries = await repository.GetSavedQueriesAsync();

        // Assert
        Assert.Equal(2, queries.Count());
        Assert.Equal(1, queries.First().Id);
        Assert.Equal("Test1", queries.First().Name);
        Assert.Equal("q1", queries.First().Query);
        Assert.Equal(2, queries.Skip(1).First().Id);
        Assert.Equal("Test2", queries.Skip(1).First().Name);
        Assert.Equal("q2", queries.Skip(1).First().Query);
    }

    [Fact]
    public async Task DeleteQueryAsync_deletes_specified_query()
    {
        // Arrange
        var repository = new SejilRepositoryMoq(Mocks.GetTestSettings());
        await repository.SaveQueryAsync(new LogQuery { Name = "Test1", Query = "q1" });

        // Act
        var result = await repository.DeleteQueryAsync("Test1");

        // Assert
        Assert.True(result);
        var queries = await repository.GetSavedQueriesAsync();
        Assert.Empty(queries);
    }

    [Fact]
    public async Task DeleteQueryAsync_returns_false_when_specified_query_does_not_exist()
    {
        // Arrange
        var repository = new SejilRepositoryMoq(Mocks.GetTestSettings());
        await repository.SaveQueryAsync(new LogQuery { Name = "Test1", Query = "q1" });

        // Act
        var result = await repository.DeleteQueryAsync("Test2");

        // Assert
        Assert.False(result);
        var queries = await repository.GetSavedQueriesAsync();
        Assert.Single(queries);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task GetEventsPageAsync_throws_when_page_arg_is_zero(int page)
    {
        // Arrange
        var repository = new SejilRepositoryMoq(Mocks.GetTestSettings());

        // Act & assert
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.GetEventsPageAsync(page, null, new LogQueryFilter()));
        Assert.Equal($"Argument must be greater than zero. (Parameter 'page')", ex.Message);
    }

    [Fact]
    public async Task GetEventsPageAsync_no_props_returns_events_page()
    {
        // Arrange
        var events = new List<LogEvent>();
        for (var i = 0; i < 10; i++)
        {
            var msgTemplate = new MessageTemplate(new[] { new TextToken($"{i}", 0) });
            events.Add(new LogEvent(DateTime.UtcNow.AddHours(i), LogEventLevel.Information, null, msgTemplate, Enumerable.Empty<LogEventProperty>()));
        }

        var repository = new SejilRepositoryMoq(Mocks.GetTestSettings(3));
        await repository.InsertEventsAsync(events);

        // Act
        var logs = await repository.GetEventsPageAsync(2, null, new LogQueryFilter());

        // Assert
        Assert.Equal(3, logs.Count());
        Assert.Equal("6", logs.ElementAt(0).Message);
        Assert.Empty(logs.ElementAt(0).Properties);
        Assert.Equal("5", logs.ElementAt(1).Message);
        Assert.Empty(logs.ElementAt(1).Properties);
        Assert.Equal("4", logs.ElementAt(2).Message);
        Assert.Empty(logs.ElementAt(2).Properties);
    }

    [Fact]
    public async Task GetEventsPageAsync_returns_events_page()
    {
        // Arrange
        var events = new List<LogEvent>();
        for (var i = 0; i < 10; i++)
        {
            var msgTemplate = new MessageTemplate(new[] { new PropertyToken("p1", "{p1}"), new PropertyToken("p2", "{p2}"), });
            var properties = new List<LogEventProperty>
            {
                new LogEventProperty("p1", new ScalarValue($"{i}_0")),
                new LogEventProperty("p2", new ScalarValue($"{i}_1")),
            };
            events.Add(new LogEvent(DateTime.UtcNow.AddHours(i), LogEventLevel.Information, null, msgTemplate, properties));
        }

        var repository = new SejilRepositoryMoq(Mocks.GetTestSettings(3));
        await repository.InsertEventsAsync(events);

        // Act
        var logs = await repository.GetEventsPageAsync(4, null, new LogQueryFilter());

        // Assert
        var log = Assert.Single(logs);
        Assert.Equal("\"0_0\"\"0_1\"", log.Message);
        Assert.NotNull(log.Properties);
        Assert.Equal(2, log.Properties.Count);
        Assert.Equal("0_0", log.Properties[0].Value);
        Assert.Equal("0_1", log.Properties[1].Value);
    }
}
