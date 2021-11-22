using System.Reflection;
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
    public static readonly bool IsCi = Environment.GetEnvironmentVariable("CI") == "true";

    public static readonly string ConnStr = IsCi
        ? Environment.GetEnvironmentVariable("SqlServerConnStr")
        : "Server=.;Database=SejilTestDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    public DbFixture()
    {
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
    }

    public void Dispose()
    {
        using var conn = new SqlConnection(ConnStr);
        conn.Execute("DELETE [sejil].[log_property]");
        conn.Execute("DELETE [sejil].[log]");
        conn.Execute("DELETE [sejil].[log_query]");
    }

    private static bool DbExists()
    {
        const string Sql = "SELECT COUNT(1) FROM master.dbo.sysdatabases WHERE name = 'SejilTestDb'";

        using var conn = new SqlConnection(ConnStr.Replace("SejilTestDb", "master"));
        conn.Open();
        return (int)conn.ExecuteScalar(Sql) == 1;
    }
}

public class SqlServerSejilRepositoryTests : IClassFixture<DbFixture>
{
    public SqlServerSejilRepositoryTests(DbFixture _) { }

    [Fact]
    public async Task AllTests()
    {
        // Arrange
        var settings = new SejilSettings("/sejil", LogEventLevel.Verbose);
        settings.UseSqlServer(DbFixture.ConnStr);
        var repository = (SqlServerSejilRepository)settings.SejilRepository;

        await repository.InsertEventsAsync(GetTestEvents());

        await repository.SaveQueryAsync(new LogQuery { Name = "TestName", Query = "TestQuery" });
        var savedQuery = Assert.Single(await repository.GetSavedQueriesAsync());
        Assert.Equal("TestName", savedQuery.Name);
        Assert.Equal("TestQuery", savedQuery.Query);

        await AssertFiltersByLevel(repository);
        await AssertFiltersByExceptionOnly(repository);
        await AssertFiltersByDate(repository);
        await AssertFiltersByDateRange(repository);
        await AssertFiltersByQuery(repository);
    }

    private static async Task AssertFiltersByLevel(SqlServerSejilRepository repository)
    {
        var e = Assert.Single(await repository.GetEventsPageAsync(1, null, new LogQueryFilter { LevelFilter = "Debug" }));
        Assert.Equal(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 11, 5, 5, 5, DateTimeKind.Local)), e.Timestamp);
        Assert.Equal("Debug", e.Level);
        Assert.Equal("Object is \"{ Id = 5, Name = Test Object }\"", e.Message);
        Assert.Null(e.Exception);
    }

    private static async Task AssertFiltersByDate(SqlServerSejilRepository repository)
        => Assert.Empty(await repository.GetEventsPageAsync(1, null, new LogQueryFilter { DateFilter = "5m" }));

    private static async Task AssertFiltersByDateRange(SqlServerSejilRepository repository)
    {
        var start = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 12, 0, 0, DateTimeKind.Local));
        var end = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 12, 10, 0, DateTimeKind.Local));

        var e = Assert.Single(await repository.GetEventsPageAsync(1, null, new LogQueryFilter { DateRangeFilter = new List<DateTime> { start, end } }));
        Assert.Equal(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 12, 5, 5, 5, DateTimeKind.Local)), e.Timestamp);
        Assert.Equal("Warning", e.Level);
        Assert.Equal("This is a warning with value: null", e.Message);
        Assert.Null(e.Exception);
    }

    private static async Task AssertFiltersByExceptionOnly(SqlServerSejilRepository repository)
    {
        var e = Assert.Single(await repository.GetEventsPageAsync(1, null, new LogQueryFilter { ExceptionsOnly = true }));
        Assert.Equal(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 13, 5, 5, 5, DateTimeKind.Local)), e.Timestamp);
        Assert.Equal("Error", e.Level);
        Assert.Equal("This is an exception", e.Message);
        Assert.Equal("System.Exception: Test exception", e.Exception);
    }

    private static async Task AssertFiltersByQuery(SqlServerSejilRepository repository)
    {
        var e = Assert.Single(await repository.GetEventsPageAsync(1, null, new LogQueryFilter { QueryText = "name = 'test name'" }));
        Assert.Equal(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2017, 8, 3, 10, 5, 5, 5, DateTimeKind.Local)), e.Timestamp);
        Assert.Equal("Information", e.Level);
        Assert.Equal("Name is \"Test name\" and Value is \"Test value\"", e.Message);
        Assert.Null(e.Exception);
    }

    private static IEnumerable<LogEvent> GetTestEvents() => new[]
    {
        BuildLogEvent(new DateTime(2017, 8, 3, 10, 5, 5, 5, DateTimeKind.Local), LogEventLevel.Information, null, "Name is {Name} and Value is {Value}", "Test name", "Test value"),
        BuildLogEvent(new DateTime(2017, 8, 3, 11, 5, 5, 5, DateTimeKind.Local), LogEventLevel.Debug, null, "Object is {Object}", new { Id = 5, Name = "Test Object" }),
        BuildLogEvent(new DateTime(2017, 8, 3, 12, 5, 5, 5, DateTimeKind.Local), LogEventLevel.Warning, null, "This is a warning with value: {Value}", (string)null),
        BuildLogEvent(new DateTime(2017, 8, 3, 13, 5, 5, 5, DateTimeKind.Local), LogEventLevel.Error, new Exception("Test exception"), "This is an exception"),
    };

    private static LogEvent BuildLogEvent(DateTime timestamp, LogEventLevel level, Exception ex, string messageTemplate, params object[] propertyValues)
    {
        var logger = new LoggerConfiguration().CreateLogger();
        logger.BindMessageTemplate(messageTemplate, propertyValues, out var parsedTemplate, out var boundProperties);
        return new LogEvent(timestamp, level, ex, parsedTemplate, boundProperties);
    }
}
