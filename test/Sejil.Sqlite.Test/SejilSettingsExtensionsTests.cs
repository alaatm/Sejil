using System.Reflection;
using Sejil.Configuration;
using Sejil.Data;
using Sejil.Sqlite.Data;
using Sejil.Sqlite.Data.Query;

namespace Sejil.Sqlite.Test;

public class SejilSettingsExtensionsTests
{
    public SejilSettingsExtensionsTests()
        => Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", string.Empty, EnvironmentVariableTarget.Process);

    [Fact]
    public void UseSqlite_sets_db_path_to_localAppData_when_running_outside_Azure()
    {
        // Arrange
        var appName = Assembly.GetEntryAssembly().GetName().Name;
        var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
        var settings = new SejilSettings("/sejil", default);

        var expectedConnStr = $"DataSource={Path.Combine(basePath, "Sejil-59A8F730-6AC5-427A-9492-A3A9EAD9556F.sqlite")}";

        // Act
        settings.UseSqlite();

        // Assert
        var actualConnStr = ReadConnStr((SejilRepository)settings.SejilRepository);
        Assert.Equal(expectedConnStr, actualConnStr);
    }

    [Fact]
    public void UseSqlite_sets_db_path_to_home_folder_when_running_inside_Azure()
    {
        // Arrange
        Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", "value", EnvironmentVariableTarget.Process);

        var dbName = Guid.NewGuid().ToString();
        var basePath = Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory), "home", $"Sejil-{dbName}.sqlite");
        var settings = new SejilSettings("/sejil", default);

        var expectedConnStr = $"DataSource={basePath}";

        // Act
        settings.UseSqlite(dbName);

        // Assert
        var actualConnStr = ReadConnStr((SejilRepository)settings.SejilRepository);
        Assert.Equal(expectedConnStr, actualConnStr);
    }

    [Fact]
    public void UseSqlite_sets_sqlite_repository()
    {
        // Arrange
        var settings = new SejilSettings("/sejil", default);

        // Act
        settings.UseSqlite();

        // Assert
        Assert.IsType<SqliteSejilRepository>(settings.SejilRepository);
    }

    [Fact]
    public void UseSqlite_sets_sqliteCodeGenerator_type()
    {
        // Arrange
        var settings = new SejilSettings("/sejil", default);

        // Act
        settings.UseSqlite();

        // Assert
        Assert.Equal(typeof(SqliteCodeGenerator), settings.CodeGeneratorType);
    }

    private static string ReadConnStr(SejilRepository repository)
        => (string)typeof(SejilRepository)
            .GetProperty("ConnectionString", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(repository);
}
