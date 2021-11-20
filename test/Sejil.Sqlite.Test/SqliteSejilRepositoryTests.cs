using Dapper;
using Microsoft.Data.Sqlite;
using Sejil.Configuration;
using Sejil.Sqlite.Data;

namespace Sejil.Sqlite.Test;

public class SqliteSejilRepositoryTests
{
    [Fact]
    public async Task InitializeDatabase_creates_database()
    {
        // Arrange
        var connStr = $"Data Source={Guid.NewGuid()}";
        var repository = new SqliteSejilRepository(Mock.Of<ISejilSettings>(), connStr);

        // Act
        await repository.GetSavedQueriesAsync(); // trigger db creation

        // Assert
        using var conn = new SqliteConnection(connStr);
        conn.Open();
        AssertTables(conn);
        AssertLogTableColumns(conn);
        AssertLogPropertyTableColumns(conn);
        AssertLogQueryTableColumns(conn);
    }

    private static void AssertTables(SqliteConnection conn)
    {
        var tables = GetTables(conn);
        Assert.Contains("log", tables);
        Assert.Contains("log_property", tables);
        Assert.Contains("log_query", tables);
    }

    private static void AssertLogTableColumns(SqliteConnection conn)
    {
        var columns = GetColumns(conn, "log");
        var fks = GetForeignKeyColumns(conn, "log");

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

    private static void AssertLogPropertyTableColumns(SqliteConnection conn)
    {
        var columns = GetColumns(conn, "log_property");
        var fks = GetForeignKeyColumns(conn, "log_property");

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

    private static void AssertLogQueryTableColumns(SqliteConnection conn)
    {
        var columns = GetColumns(conn, "log_query");
        var fks = GetForeignKeyColumns(conn, "log_query");

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

    private static IEnumerable<string> GetTables(SqliteConnection conn)
        => conn.Query<string>("SELECT name FROM sqlite_master WHERE type='table';");

    private static IEnumerable<ColumnInfo> GetColumns(SqliteConnection conn, string tableName)
        => conn.Query<ColumnInfo>($"pragma table_info('{tableName}');");

    private static IEnumerable<ForeignKeyColumnInfo> GetForeignKeyColumns(SqliteConnection conn, string tableName)
        => conn.Query<ForeignKeyColumnInfo>($"pragma foreign_key_list('{tableName}');");

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
}
