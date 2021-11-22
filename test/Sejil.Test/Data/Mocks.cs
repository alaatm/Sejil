using System.Data.Common;
using Microsoft.Data.Sqlite;
using Sejil.Configuration;
using Sejil.Data;
using Sejil.Data.Query;
using Serilog.Events;

namespace Sejil.Test.Data;

public static class Mocks
{
    public static void UseMockStore(this ISejilSettings settings)
    {
        settings.CodeGeneratorType = typeof(CodeGeneratorMoq);
        settings.SejilRepository = new SejilRepositoryMoq(settings);
    }

    public static ISejilSettings GetTestSettings(int pageSize = 100) => new SejilSettings("/sejil", LogEventLevel.Information)
    {
        CodeGeneratorType = typeof(CodeGeneratorMoq),
        PageSize = pageSize,
    };
}

public class SejilRepositoryMoq : SejilRepository
{
    protected override string LogTableName { get; } = "log";
    protected override string LogPropertyTableName { get; } = "log_property";
    protected override string LogQueryTableName { get; } = "log_query";

    public SejilRepositoryMoq(ISejilSettings settings) : base(settings, $"DataSource={Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())}") { }
    protected override void InitializeDatabase()
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = TestDb.GetSqliteInitSql();
        cmd.ExecuteNonQuery();
    }
    protected override DbConnection GetConnection() => new SqliteConnection(ConnectionString);
    protected override string GetPaginSql(int offset, int take) => $"LIMIT {take} OFFSET {offset}";
    protected override string GetDateTimeOffsetSql(int value, string unit) => $"datetime('now', '{value} {unit}')";
}

public class CodeGeneratorMoq : CodeGenerator
{
    protected override string LogPropertyTableName { get; } = "log_property";
    protected override string NumericCastSql { get; } = "CAST(value AS NUMERIC)";
    protected override string PropertyFilterNegateSql { get; } = "SUM(name = '|PNAME|' AND |VALCOL| |OP| |PVAL|) = 0";
    protected override string PropertyFilterSql { get; } = "SUM(name = '|PNAME|' AND |VALCOL| |OP| |PVAL|) > 0";
}

public static class TestDb
{
    public static string GetSqliteInitSql() => @"CREATE TABLE IF NOT EXISTS log(
	id              TEXT        NOT NULL    PRIMARY KEY,
	message         TEXT        NOT NULL    COLLATE NOCASE,
	messageTemplate TEXT        NOT NULL,
	level           VARCHAR(64) NOT NULL,
	timestamp       DATETIME    NOT NULL,
	exception       TEXT        NULL        COLLATE NOCASE
);

CREATE INDEX IF NOT EXISTS log_message_idx		ON log(message);

CREATE INDEX IF NOT EXISTS log_level_idx		ON log(level);

CREATE INDEX IF NOT EXISTS log_timestamp_idx	ON log(timestamp);

CREATE INDEX IF NOT EXISTS log_exception_idx	ON log(exception);

CREATE TABLE IF NOT EXISTS log_property(
	id      INTEGER NOT NULL    PRIMARY KEY AUTOINCREMENT,
	logId   TEXT    NOT NULL,
	name    TEXT    NOT NULL    COLLATE NOCASE,
	value   TEXT    NULL        COLLATE NOCASE,
	FOREIGN KEY(logId) REFERENCES log(id)
);

CREATE INDEX IF NOT EXISTS log_property_logId_idx	ON log_property(logId);

CREATE INDEX IF NOT EXISTS log_property_name_idx	ON log_property(name);

CREATE INDEX IF NOT EXISTS log_property_value_idx	ON log_property(value);

CREATE TABLE IF NOT EXISTS log_query(
	id      INTEGER      NOT NULL PRIMARY KEY AUTOINCREMENT,
	name    VARCHAR(255) NOT NULL,
	query   TEXT         NOT NULL
);

CREATE INDEX IF NOT EXISTS log_query_name_idx ON log_query(name);";
}
