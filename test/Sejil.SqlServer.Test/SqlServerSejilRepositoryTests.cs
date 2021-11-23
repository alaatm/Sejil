using System.Reflection;
using System.Runtime.InteropServices;
using Dapper;
using Microsoft.Data.SqlClient;
using Sejil.Configuration;
using Sejil.Models;
using Sejil.SqlServer.Data;
using Serilog;
using Serilog.Events;

namespace Sejil.SqlServer.Test;

public sealed class DbFixture : IDisposable
{
    public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static readonly bool IsCi = Environment.GetEnvironmentVariable("CI") == "true";

    public static readonly string ConnStr = IsCi
        ? Environment.GetEnvironmentVariable("ConnStr")
        : "Server=.;Database=SejilTestDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    internal SqlServerSejilRepository Repository { get; }

    public DbFixture()
    {
        if (IsCi && IsWindows)
        {
            return;
        }

        var mdf = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SejilTestDbData.mdf");
        if (!DbExists())
        {
            var sql =
$@"CREATE DATABASE SejilTestDb ON PRIMARY 
(NAME = SejilTestDb_Data, FILENAME = '{mdf}', 
SIZE = 1MB, MAXSIZE = 2MB, FILEGROWTH = 10%)";

            var connStr = ConnStr.Replace("SejilTestDb", "master");

            using var conn = new SqlConnection(connStr);
            conn.Open();
            conn.Execute(sql);
        }

        var settings = new SejilSettings("/sejil", LogEventLevel.Verbose);
        settings.UseSqlServer(ConnStr);
        Repository = (SqlServerSejilRepository)settings.SejilRepository;

        DeleteData();
        Seed();
    }

    private void Seed()
        => Repository.InsertEventsAsync(GetTestEvents()).GetAwaiter().GetResult();

    public void Dispose() => DeleteData();

    private static void DeleteData()
    {
        using var conn = new SqlConnection(ConnStr);
        conn.Execute(
@"IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES)
EXEC('DELETE [sejil].[log_property];DELETE [sejil].[log];DELETE [sejil].[log_query];')");
    }

    private static bool DbExists()
    {
        const string Sql = "SELECT COUNT(1) FROM master.dbo.sysdatabases WHERE name = 'SejilTestDb'";

        using var conn = new SqlConnection(ConnStr.Replace("SejilTestDb", "master"));
        conn.Open();
        return (int)conn.ExecuteScalar(Sql) == 1;
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

public class SqlServerSejilRepositoryTests : IClassFixture<DbFixture>
{
    private readonly SqlServerSejilRepository _repository;

    public SqlServerSejilRepositoryTests(DbFixture db)
        => _repository = db.Repository;

    [SkippableFact]
    public async Task Can_save_load_delete_queries()
    {
        Skip.If(DbFixture.IsCi && DbFixture.IsWindows);

        var queryName = "TestName";

        await _repository.SaveQueryAsync(new LogQuery { Name = queryName, Query = "TestQuery" });
        var savedQuery = Assert.Single(await _repository.GetSavedQueriesAsync());
        Assert.Equal(queryName, savedQuery.Name);
        Assert.Equal("TestQuery", savedQuery.Query);

        await _repository.DeleteQueryAsync(queryName);
        Assert.Empty(await _repository.GetSavedQueriesAsync());
    }

    [SkippableFact]
    public async Task Can_filter_by_level()
    {
        Skip.If(DbFixture.IsCi && DbFixture.IsWindows);

        var e = Assert.Single(await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { LevelFilter = "Debug" }));
        Assert.Equal(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 11, 5, 5, 5, DateTimeKind.Local)), e.Timestamp);
        Assert.Equal("Debug", e.Level);
        Assert.Equal("Object is \"{ Id = 5, Name = Test Object }\"", e.Message);
        Assert.Null(e.Exception);
    }

    [SkippableFact]
    public async Task Can_filter_by_date()
    {
        Skip.If(DbFixture.IsCi && DbFixture.IsWindows);
        Assert.Empty(await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { DateFilter = "5m" }));
    }

    [SkippableFact]
    public async Task Can_filter_by_dateRange()
    {
        Skip.If(DbFixture.IsCi && DbFixture.IsWindows);

        var start = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 12, 0, 0, DateTimeKind.Local));
        var end = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 12, 10, 0, DateTimeKind.Local));

        var e = Assert.Single(await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { DateRangeFilter = new List<DateTime> { start, end } }));
        Assert.Equal(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 12, 5, 5, 5, DateTimeKind.Local)), e.Timestamp);
        Assert.Equal("Warning", e.Level);
        Assert.Equal("This is a warning with value: null", e.Message);
        Assert.Null(e.Exception);
    }

    [SkippableFact]
    public async Task Can_filters_by_exceptionOnly()
    {
        Skip.If(DbFixture.IsCi && DbFixture.IsWindows);

        var e = Assert.Single(await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { ExceptionsOnly = true }));
        Assert.Equal(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 13, 5, 5, 5, DateTimeKind.Local)), e.Timestamp);
        Assert.Equal("Error", e.Level);
        Assert.Equal("This is an exception", e.Message);
        Assert.Equal("System.Exception: Test exception", e.Exception);
    }

    [SkippableFact]
    public async Task Can_filter_by_query()
    {
        Skip.If(DbFixture.IsCi && DbFixture.IsWindows);

        var e = Assert.Single(await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { QueryText = "name = 'test name'" }));
        Assert.Equal(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 10, 5, 5, 5, DateTimeKind.Local)), e.Timestamp);
        Assert.Equal("Information", e.Level);
        Assert.Equal("Name is \"Test name\" and Value is \"Test value\"", e.Message);
        Assert.Null(e.Exception);
    }

    [SkippableFact]
    public async Task CleanupAsync_test()
    {
        Skip.If(DbFixture.IsCi && DbFixture.IsWindows);

        // Arrange
        var settings = (ISejilSettings)typeof(SqlServerSejilRepository)
            .GetProperty("Settings", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(_repository);

        settings
            .AddRetentionPolicy(TimeSpan.FromHours(5), LogEventLevel.Verbose, LogEventLevel.Debug)
            .AddRetentionPolicy(TimeSpan.FromDays(10), LogEventLevel.Information)
            .AddRetentionPolicy(TimeSpan.FromDays(75));

        var now = DateTime.UtcNow;
        await _repository.InsertEventsAsync(new[]
        {
            DbFixture.BuildLogEvent(now.AddHours(-5.1), LogEventLevel.Verbose, null, "Verbose #{Num}", 1),
            DbFixture.BuildLogEvent(now.AddHours(-5.1), LogEventLevel.Debug, null, "Debug #{Num}", 1),
            DbFixture.BuildLogEvent(now, LogEventLevel.Verbose, null, "Verbose #{Num}", 2),
            DbFixture.BuildLogEvent(now, LogEventLevel.Debug, null, "Debug #{Num}", 2),

            DbFixture.BuildLogEvent(now.AddDays(-10.1), LogEventLevel.Information, null, "Information #{Num}", 1),
            DbFixture.BuildLogEvent(now, LogEventLevel.Information, null, "Information #{Num}", 2),

            DbFixture.BuildLogEvent(now.AddDays(-75.1), LogEventLevel.Warning, null, "Warning #{Num}", 1),
            DbFixture.BuildLogEvent(now.AddDays(-75.1), LogEventLevel.Error, null, "Error #{Num}", 1),
            DbFixture.BuildLogEvent(now.AddDays(-75.1), LogEventLevel.Fatal, null, "Fatal #{Num}", 1),
            DbFixture.BuildLogEvent(now, LogEventLevel.Warning, null, "Warning #{Num}", 2),
            DbFixture.BuildLogEvent(now, LogEventLevel.Error, null, "Error #{Num}", 2),
            DbFixture.BuildLogEvent(now, LogEventLevel.Fatal, null, "Fatal #{Num}", 2),
        });

        // Act
        Assert.Equal(6, (await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { QueryText = "Num=1" })).Count());
        await _repository.CleanupAsync();

        // Assert
        Assert.Empty(await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { QueryText = "Num=1" }));
        Assert.Equal(6, (await _repository.GetEventsPageAsync(1, null, new LogQueryFilter { QueryText = "Num=2" })).Count());

        //// Cleanup
        //using var conn = new SqlConnection(DbFixture.ConnStr);
        //await conn.OpenAsync();
        //await conn.ExecuteAsync("DELETE FROM [sejil].[log] WHERE message like '%#2'");
    }
}
