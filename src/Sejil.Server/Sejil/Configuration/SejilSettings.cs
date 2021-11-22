// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Sejil.Data;
using Serilog.Core;
using Serilog.Events;

namespace Sejil.Configuration;

public sealed class SejilSettings : ISejilSettings
{
    private int _pageSize = 100;

    /// <summary>
    /// Gets or sets the authentication scheme, used for the index page. Leave empty for no authentication.
    /// </summary>
    public string? AuthenticationScheme { get; set; }
    /// <summary>
    /// Gets the logging level switch.
    /// </summary>
    public LoggingLevelSwitch LoggingLevelSwitch { get; private set; }
    /// <summary>
    /// Gets the Sejil front-end html.
    /// </summary>
    public string SejilAppHtml { get; private set; }
    /// <summary>
    /// Gets or sets the title shown in the front end
    /// </summary>
    public string Title { get; set; } = "Sejil";
    /// <summary>
    /// Gets the configured Sejil Url.
    /// </summary>
    public string Url { get; private set; }
    /// <summary>
    /// Gets or sets the sejil repository.
    /// </summary>
    /// <remarks>
    /// This is meant to be used only by store providers.
    /// </remarks>
    public SejilRepository SejilRepository { get; set; } = default!;
    /// <summary>
    /// Gets or sets the sejil code generator clr type.
    /// </summary>
    /// <remarks>
    /// This is meant to be used only by store providers.
    /// </remarks>
    public Type CodeGeneratorType { get; set; } = default!;

    /// <summary>
    /// Gets or sets the logs page size in the front-end grid, defaults to 100.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _pageSize = value;
        }
    }

    public SejilSettings(string uri, LogEventLevel minLogLevel)
    {
        Url = uri.StartsWith("/", StringComparison.Ordinal)
            ? uri
            : $"/{uri}";

        SejilAppHtml = ResourceHelper.GetEmbeddedResource(typeof(SejilSettings).Assembly, "Sejil.index.html");
        LoggingLevelSwitch = new LoggingLevelSwitch
        {
            MinimumLevel = minLogLevel
        };
    }

    /// <summary>
    /// Sets the minimum log level.
    /// </summary>
    /// <remarks>
    /// This is not meant to be used by user-code.
    /// </remarks>
    /// <param name="minLogLevel">The min log level.</param>
    /// <returns></returns>
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
