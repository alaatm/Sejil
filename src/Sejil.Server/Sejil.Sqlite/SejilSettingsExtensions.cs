using System.Reflection;
using Sejil.Configuration.Internal;
using Sejil.Sqlite.Data.Internal;
using Sejil.Sqlite.Data.Query.Internal;

namespace Sejil;

public static class SejilSettingsExtensions
{
    private const string UUID = "59A8F730-6AC5-427A-9492-A3A9EAD9556F";

    public static ISejilSettings UseSqlite(this ISejilSettings settings, string? name = null)
    {
        var settingsInstance = (SejilSettings)settings;

        string sqliteDbPath;

        if (IsRunningInAzure())
        {
            sqliteDbPath = Path.Combine(Path.GetFullPath("/home"), $"Sejil-{name ?? UUID}.sqlite");
        }
        else
        {
            var localAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appName = Assembly.GetEntryAssembly()!.GetName().Name!;
            sqliteDbPath = Path.Combine(localAppFolder, appName, $"Sejil-{name ?? UUID}.sqlite");
        }

        var sqliteDbFile = new FileInfo(sqliteDbPath);
        sqliteDbFile.Directory!.Create();

        settingsInstance.SejilRepository = new SqliteSejilRepository(settingsInstance, $"DataSource={sqliteDbPath}");
        settingsInstance.CodeGeneratorType = typeof(SqliteCodeGenerator);

        return settings;
    }

    private static bool IsRunningInAzure()
        => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
}
