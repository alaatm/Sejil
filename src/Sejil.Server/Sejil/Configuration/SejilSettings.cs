// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Sejil.Data;
using Serilog.Core;
using Serilog.Events;

namespace Sejil.Configuration;

internal sealed class SejilSettings : ISejilSettings
{
    internal SejilRepository SejilRepository { get; set; } = default!;
    internal Type CodeGeneratorType { get; set; } = default!;

    public string SejilAppHtml { get; private set; }
    public string Url { get; private set; }
    public LoggingLevelSwitch LoggingLevelSwitch { get; private set; }
    public int PageSize { get; private set; }

    /// <summary>
    /// Gets or sets the title shown in the front end
    /// </summary>
    public string Title { get; set; } = "Sejil";

    /// <summary>
    /// Gets or sets the authentication scheme, used for the index page. Leave empty for no authentication.
    /// </summary>
    public string? AuthenticationScheme { get; set; }

    public SejilSettings(string uri, LogEventLevel minLogLevel, int pageSize = 100)
    {
        Url = uri.StartsWith("/", StringComparison.Ordinal)
            ? uri
            : $"/{uri}";

        SejilAppHtml = ResourceHelper.GetEmbeddedResource("Sejil.index.html");
        LoggingLevelSwitch = new LoggingLevelSwitch
        {
            MinimumLevel = minLogLevel
        };

        PageSize = pageSize;
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
}
