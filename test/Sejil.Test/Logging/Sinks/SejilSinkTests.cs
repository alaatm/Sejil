// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Microsoft.Data.Sqlite;
using Sejil.Configuration.Internal;
using Sejil.Logging.Sinks;
using Dapper;
using Serilog.Events;
using Serilog.Parsing;
using Sejil.Data.Internal;

namespace Sejil.Test.Logging.Sinks;

public class SejilSinkTests
{
    [Fact]
    public void Sink_creates_database_tables()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        var settingsMoq = new Mock<ISejilSettings>();
        settingsMoq.SetupGet(p => p.SqliteDbPath).Returns(db);

        // Act
        var sink = new SejilSink(settingsMoq.Object);

        // Assert
        var tables = GetTables(db);
        Assert.Contains("log", tables);
        Assert.Contains("log_property", tables);
        Assert.Contains("log_query", tables);
    }

    [Fact]
    public void Sink_creates_log_table_columns()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        var settingsMoq = new Mock<ISejilSettings>();
        settingsMoq.SetupGet(p => p.SqliteDbPath).Returns(db);

        // Act
        var sink = new SejilSink(settingsMoq.Object);

        // Assert
        var columns = GetColumns(db, "log");
        var fks = GetForeignKeyColumns(db, "log");

        Assert.Equal(6, columns.Count());
        Assert.Empty(fks);

        var idCol = columns.ElementAt(0);
        Assert.Equal("id", idCol.Name);
        Assert.Equal("TEXT", idCol.Type);
        Assert.True(idCol.NotNull);
        Assert.True(idCol.Pk);

        var messageCol = columns.ElementAt(1);
        Assert.Equal("message", messageCol.Name);
        Assert.Equal("TEXT", messageCol.Type);
        Assert.True(messageCol.NotNull);
        Assert.False(messageCol.Pk);

        var messageTemplateCol = columns.ElementAt(2);
        Assert.Equal("messageTemplate", messageTemplateCol.Name);
        Assert.Equal("TEXT", messageTemplateCol.Type);
        Assert.True(messageTemplateCol.NotNull);
        Assert.False(messageTemplateCol.Pk);

        var levelCol = columns.ElementAt(3);
        Assert.Equal("level", levelCol.Name);
        Assert.Equal("VARCHAR(64)", levelCol.Type);
        Assert.True(levelCol.NotNull);
        Assert.False(levelCol.Pk);

        var timestampCol = columns.ElementAt(4);
        Assert.Equal("timestamp", timestampCol.Name);
        Assert.Equal("DATETIME", timestampCol.Type);
        Assert.True(timestampCol.NotNull);
        Assert.False(timestampCol.Pk);

        var exceptionCol = columns.ElementAt(5);
        Assert.Equal("exception", exceptionCol.Name);
        Assert.Equal("TEXT", exceptionCol.Type);
        Assert.False(exceptionCol.NotNull);
        Assert.False(exceptionCol.Pk);
    }

    [Fact]
    public void Sink_creates_log_property_table_columns()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        var settingsMoq = new Mock<ISejilSettings>();
        settingsMoq.SetupGet(p => p.SqliteDbPath).Returns(db);

        // Act
        var sink = new SejilSink(settingsMoq.Object);

        // Assert
        var columns = GetColumns(db, "log_property");
        var fks = GetForeignKeyColumns(db, "log_property");

        Assert.Equal(4, columns.Count());
        Assert.Single(fks);

        var idCol = columns.ElementAt(0);
        Assert.Equal("id", idCol.Name);
        Assert.Equal("INTEGER", idCol.Type);
        Assert.True(idCol.NotNull);
        Assert.True(idCol.Pk);

        var logIdCol = columns.ElementAt(1);
        Assert.Equal("logId", logIdCol.Name);
        Assert.Equal("TEXT", logIdCol.Type);
        Assert.True(logIdCol.NotNull);
        Assert.False(logIdCol.Pk);

        var nameCol = columns.ElementAt(2);
        Assert.Equal("name", nameCol.Name);
        Assert.Equal("TEXT", nameCol.Type);
        Assert.True(nameCol.NotNull);
        Assert.False(nameCol.Pk);

        var valueCol = columns.ElementAt(3);
        Assert.Equal("value", valueCol.Name);
        Assert.Equal("TEXT", valueCol.Type);
        Assert.False(valueCol.NotNull);
        Assert.False(valueCol.Pk);

        var fkCol = fks.ElementAt(0);
        Assert.Equal("log", fkCol.Table);
        Assert.Equal("logId", fkCol.From);
        Assert.Equal("id", fkCol.To);
    }

    [Fact]
    public void Sink_creates_log_query_table_columns()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        var settingsMoq = new Mock<ISejilSettings>();
        settingsMoq.SetupGet(p => p.SqliteDbPath).Returns(db);

        // Act
        var sink = new SejilSink(settingsMoq.Object);

        // Assert
        var columns = GetColumns(db, "log_query");
        var fks = GetForeignKeyColumns(db, "log_query");

        Assert.Equal(3, columns.Count());
        Assert.Empty(fks);

        var idCol = columns.ElementAt(0);
        Assert.Equal("id", idCol.Name);
        Assert.Equal("INTEGER", idCol.Type);
        Assert.True(idCol.NotNull);
        Assert.True(idCol.Pk);

        var nameCol = columns.ElementAt(1);
        Assert.Equal("name", nameCol.Name);
        Assert.Equal("VARCHAR(255)", nameCol.Type);
        Assert.True(nameCol.NotNull);
        Assert.False(nameCol.Pk);

        var queryCol = columns.ElementAt(2);
        Assert.Equal("query", queryCol.Name);
        Assert.Equal("TEXT", queryCol.Type);
        Assert.True(queryCol.NotNull);
        Assert.False(queryCol.Pk);
    }

    [Fact]
    public async Task EmitBatchAsync_throws_when_null_events()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        var settingsMoq = new Mock<ISejilSettings>();
        settingsMoq.SetupGet(p => p.SqliteDbPath).Returns(db);
        settingsMoq.SetupGet(p => p.PageSize).Returns(100);
        settingsMoq.SetupGet(p => p.Url).Returns("/sejil");
        var repository = new SejilRepository(new SejilSqlProvider(settingsMoq.Object), settingsMoq.Object);
        var sink = new SejilSinkMock(settingsMoq.Object);

        // Act & assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => await sink.CallEmitBatchAsync(null));
        Assert.Equal("events", ex.ParamName);
    }

    [Fact]
    public async Task EmitBatchAsync_inserts_events_to_database()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        var settingsMoq = new Mock<ISejilSettings>();
        settingsMoq.SetupGet(p => p.SqliteDbPath).Returns(db);
        settingsMoq.SetupGet(p => p.PageSize).Returns(100);
        var repository = new SejilRepository(new SejilSqlProvider(settingsMoq.Object), settingsMoq.Object);
        var sink = new SejilSinkMock(settingsMoq.Object);

        // Hello, {name}. Your # is {number}
        var tokens = new List<MessageTemplateToken>
            {
                new TextToken("Hello, ", 0),
                new PropertyToken("name", "{name}"),
                new TextToken(". Your # is ", 13),
                new PropertyToken("number", "{number}"),
            };

        var properties = new List<LogEventProperty>
            {
                new LogEventProperty("name", new ScalarValue("world")),
                new LogEventProperty("number", new ScalarValue(null))
            };

        var messageTemplate = new MessageTemplate(tokens);

        var timestamp1 = new DateTime(2017, 8, 3, 11, 44, 15, 542, DateTimeKind.Local);
        var timestamp2 = new DateTime(2017, 9, 3, 11, 44, 15, 542, DateTimeKind.Local);

        var events = new List<LogEvent>
            {
                new LogEvent(timestamp1, LogEventLevel.Information, null, messageTemplate, properties),
                new LogEvent(timestamp2, LogEventLevel.Debug, new Exception("error"), messageTemplate, properties),
            };

        // Act
        await sink.CallEmitBatchAsync(events);

        // Assert
        var logEvents = await repository.GetEventsPageAsync(1, null, null);
        Assert.Equal(2, logEvents.Count());

        var logEvent1 = logEvents.FirstOrDefault(p => p.Level == "Information");
        Assert.Equal("Hello, \"world\". Your # is null", logEvent1.Message);
        Assert.Equal("Hello, {name}. Your # is {number}", logEvent1.MessageTemplate);
        Assert.Equal("Information", logEvent1.Level);
        Assert.Equal(timestamp1, logEvent1.Timestamp);
        Assert.Null(logEvent1.Exception);
        Assert.Equal(2, logEvent1.Properties.Count);
        Assert.Equal(logEvent1.Id, logEvent1.Properties.ElementAt(0).LogId);
        Assert.Equal("name", logEvent1.Properties.ElementAt(0).Name);
        Assert.Equal("world", logEvent1.Properties.ElementAt(0).Value);
        Assert.Equal(logEvent1.Id, logEvent1.Properties.ElementAt(1).LogId);
        Assert.Equal("number", logEvent1.Properties.ElementAt(1).Name);
        Assert.Equal("null", logEvent1.Properties.ElementAt(1).Value);

        var logEvent2 = logEvents.FirstOrDefault(p => p.Level == "Debug");
        Assert.Equal("Hello, \"world\". Your # is null", logEvent2.Message);
        Assert.Equal("Hello, {name}. Your # is {number}", logEvent2.MessageTemplate);
        Assert.Equal("Debug", logEvent2.Level);
        Assert.Equal(timestamp2, logEvent2.Timestamp);
        Assert.Equal("System.Exception: error", logEvent2.Exception);
        Assert.Equal(2, logEvent2.Properties.Count);
        Assert.Equal(logEvent2.Id, logEvent2.Properties.ElementAt(0).LogId);
        Assert.Equal("name", logEvent2.Properties.ElementAt(0).Name);
        Assert.Equal("world", logEvent2.Properties.ElementAt(0).Value);
        Assert.Equal(logEvent2.Id, logEvent2.Properties.ElementAt(1).LogId);
        Assert.Equal("number", logEvent2.Properties.ElementAt(1).Name);
        Assert.Equal("null", logEvent2.Properties.ElementAt(1).Value);
    }

    [Theory]
    [InlineData("RequestPath")]
    [InlineData("Path")]
    public async Task EmitBatchAsync_ignores_events_with_sejil_url_in_RequestPath_or_Path_properties(string propertyName)
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        var settingsMoq = new Mock<ISejilSettings>();
        settingsMoq.SetupGet(p => p.SqliteDbPath).Returns(db);
        settingsMoq.SetupGet(p => p.PageSize).Returns(100);
        settingsMoq.SetupGet(p => p.Url).Returns("/sejil");
        var repository = new SejilRepository(new SejilSqlProvider(settingsMoq.Object), settingsMoq.Object);
        var sink = new SejilSinkMock(settingsMoq.Object);

        var tokens = new List<MessageTemplateToken>
            {
                new PropertyToken(propertyName, "{"+propertyName+"}"),
            };

        var properties = new List<LogEventProperty>
            {
                new LogEventProperty(propertyName, new ScalarValue("/sejil/events")),
            };

        var messageTemplate = new MessageTemplate(tokens);

        var events = new List<LogEvent>
            {
                new LogEvent(DateTime.Now, LogEventLevel.Information, null, messageTemplate, properties),
            };

        // Act
        await sink.CallEmitBatchAsync(events);

        // Assert
        var logEvents = await repository.GetEventsPageAsync(1, null, null);
        Assert.Empty(logEvents);
    }

    private static IEnumerable<string> GetTables(string db)
    {
        using var conn = new SqliteConnection($"DataSource={db}");
        conn.Open();
        return conn.Query<string>("SELECT name FROM sqlite_master WHERE type='table';");
    }

    private static IEnumerable<ColumnInfo> GetColumns(string db, string tableName)
    {
        using var conn = new SqliteConnection($"DataSource={db}");
        conn.Open();
        return conn.Query<ColumnInfo>($"pragma table_info('{tableName}');");
    }

    private static IEnumerable<ForeignKeyColumnInfo> GetForeignKeyColumns(string db, string tableName)
    {
        using var conn = new SqliteConnection($"DataSource={db}");
        conn.Open();
        return conn.Query<ForeignKeyColumnInfo>($"pragma foreign_key_list('{tableName}');");
    }

    class ColumnInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool NotNull { get; set; }
        public bool Pk { get; set; }
    }

    class ForeignKeyColumnInfo
    {
        public string Table { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }

    class SejilSinkMock : SejilSink
    {
        public SejilSinkMock(ISejilSettings settings) : base(settings) { }
        public Task CallEmitBatchAsync(IEnumerable<LogEvent> events) => EmitBatchAsync(events);
    }
}
