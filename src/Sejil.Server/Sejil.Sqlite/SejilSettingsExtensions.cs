// Copyright (C) 2021 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Reflection;
using Sejil.Configuration;
using Sejil.Sqlite.Data;
using Sejil.Sqlite.Data.Query;

namespace Sejil;

public static class SejilSettingsExtensions
{
    private const string UUID = "59A8F730-6AC5-427A-9492-A3A9EAD9556F";

    public static ISejilSettings UseSqlite(this ISejilSettings settings, string? name = null)
    {
        string sqliteDbPath;
        name ??= UUID;

        if (IsRunningInAzure())
        {
            sqliteDbPath = Path.Combine(Path.GetFullPath("/home"), $"Sejil-{name}.sqlite");
        }
        else
        {
            var localAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appName = Assembly.GetEntryAssembly()!.GetName().Name!;
            sqliteDbPath = Path.Combine(localAppFolder, appName, $"Sejil-{name}.sqlite");
        }

        var sqliteDbFile = new FileInfo(sqliteDbPath);
        sqliteDbFile.Directory!.Create();

        settings.CodeGeneratorType = typeof(SqliteCodeGenerator);
        settings.SejilRepository = new SqliteSejilRepository(settings, $"DataSource={sqliteDbPath}");

        return settings;
    }

    private static bool IsRunningInAzure()
        => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
}