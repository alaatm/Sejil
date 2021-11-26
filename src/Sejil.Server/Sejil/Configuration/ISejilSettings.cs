// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Sejil.Data;
using Serilog.Core;
using Serilog.Events;

namespace Sejil.Configuration;

public interface ISejilSettings
{
    /// <summary>
    /// Gets or sets the authentication scheme, used for the index page. Leave empty for no authentication.
    /// </summary>
    string? AuthenticationScheme { get; set; }
    /// <summary>
    /// Gets the logging level switch.
    /// </summary>
    LoggingLevelSwitch LoggingLevelSwitch { get; }
    /// <summary>
    /// The invoke duration in minutes for the cleanup background task.
    /// </summary>
    int MinimumSchedulerTimerInMinutes { get; }
    /// <summary>
    /// Gets or sets the logs page size in the front-end grid.
    /// Defaults to 100.
    /// </summary>
    int PageSize { get; set; }
    /// <summary>
    /// The list of configured retention policies
    /// </summary>
    IReadOnlyList<RetentionPolicy> RetentionPolicies { get; }
    /// <summary>
    /// Gets the Sejil front-end html.
    /// </summary>
    string SejilAppHtml { get; }
    /// <summary>
    /// Gets or sets the title shown in the front end.
    /// </summary>
    string Title { get; set; }
    /// <summary>
    /// Gets the configured Sejil Url.
    /// </summary>
    string Url { get; }
    /// <summary>
    /// Gets or sets the sejil repository.
    /// </summary>
    /// <remarks>
    /// This is meant to be used only by store providers.
    /// </remarks>
    public SejilRepository SejilRepository { get; set; }
    /// <summary>
    /// Gets or sets the sejil code generator clr type.
    /// </summary>
    /// <remarks>
    /// This is meant to be used only by store providers.
    /// </remarks>
    public Type CodeGeneratorType { get; set; }
    /// <summary>
    /// Adds a new retention policy.
    /// </summary>
    /// <param name="age">The log event age at which it should be deleted.</param>
    /// <param name="logLevels">
    /// The list of log levels to match to trigger event deletion. Leave empty to matches all log levels.
    /// </param>
    /// <returns></returns>
    ISejilSettings AddRetentionPolicy(TimeSpan age, params LogEventLevel[] logLevels);
    /// <summary>
    /// Sets the minimum log level.
    /// </summary>
    /// <remarks>
    /// This is not meant to be used by user-code.
    /// </remarks>
    /// <param name="minLogLevel">The min log level.</param>
    /// <returns></returns>
    bool TrySetMinimumLogLevel(string minLogLevel);
}
