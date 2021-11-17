// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Reflection;
using Serilog.Core;
using Serilog.Events;

namespace Sejil.Configuration.Internal;

public sealed class SejilSettings : ISejilSettings
{
    private const string UUID = "59A8F730-6AC5-427A-9492-A3A9EAD9556F";

    public string SejilAppHtml { get; private set; }
    public Uri Url { get; private set; }
    public LoggingLevelSwitch LoggingLevelSwitch { get; private set; }
    public string SqliteDbPath { get; private set; }
    public int PageSize { get; private set; }

    /// <summary>
    /// Gets or sets the title shown in the front end
    /// </summary>
    public string Title { get; set; } = "Sejil";

    /// <summary>
    /// Gets or sets the authentication scheme, used for the index page. Leave empty for no authentication.
    /// </summary>
    public string AuthenticationScheme { get; set; }

    public SejilSettings(string uri, LogEventLevel minLogLevel)
        : this(new Uri(uri, UriKind.Relative), minLogLevel) { }

    private SejilSettings(Uri uri, LogEventLevel minLogLevel)
    {
        Url = uri.OriginalString.StartsWith("/", StringComparison.Ordinal)
            ? uri
            : new Uri($"/{uri.OriginalString}", UriKind.Relative);

        SejilAppHtml = ResourceHelper.GetEmbeddedResource("Sejil.index.html");
        LoggingLevelSwitch = new LoggingLevelSwitch
        {
            MinimumLevel = minLogLevel
        };

        if (IsRunningInAzure())
        {
            // If running in azure, we won't use local app folder as its temporary and will frequently be deleted.
            // Use home folder instead.
            SqliteDbPath = Path.Combine(Path.GetFullPath("/home"), $"Sejil-{UUID}.sqlite");
        }
        else
        {
            var localAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appName = Assembly.GetEntryAssembly().GetName().Name;
            SqliteDbPath = Path.Combine(localAppFolder, appName, $"Sejil-{UUID}.sqlite");
        }

        PageSize = 100;
    }

    public bool TrySetMinimumLogLevel(string minLogLevel)
    {
        switch (minLogLevel.ToUpperInvariant())
        {
            case "TRACE":
            case "VERBOSE":
                LoggingLevelSwitch.MinimumLevel = LogEventLevel.Verbose;
                return true;
            case "DEBUG":
                LoggingLevelSwitch.MinimumLevel = LogEventLevel.Debug;
                return true;
            case "INFORMATION":
                LoggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
                return true;
            case "WARNING":
                LoggingLevelSwitch.MinimumLevel = LogEventLevel.Warning;
                return true;
            case "ERROR":
                LoggingLevelSwitch.MinimumLevel = LogEventLevel.Error;
                return true;
            case "CRITICAL":
            case "FATAL":
                LoggingLevelSwitch.MinimumLevel = LogEventLevel.Fatal;
                return true;
            default:
                return false;
        }
    }

    private static bool IsRunningInAzure()
        => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
}
