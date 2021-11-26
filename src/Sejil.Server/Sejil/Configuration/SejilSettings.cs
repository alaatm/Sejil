// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Sejil.Data;
using Serilog.Core;
using Serilog.Events;

namespace Sejil.Configuration;

public sealed class SejilSettings : ISejilSettings
{
    private readonly List<RetentionPolicy> _retentionPolicies = new();
    private int _pageSize = 100;

    /// <summary>
    /// The invoke duration in minutes for the cleanup background task.
    /// </summary>
    public int MinimumSchedulerTimerInMinutes
    {
        get
        {
            if (!_retentionPolicies.Any())
            {
                return -1;
            }

            var pfs = new Dictionary<int, List<int>>();
            foreach (var m in _retentionPolicies.Select(p => (int)p.Age.TotalMinutes))
            {
                foreach (var kvp in GetPrimeFactors(m))
                {
                    if (!pfs.ContainsKey(kvp.Key))
                    {
                        pfs[kvp.Key] = new List<int>();
                    }
                    pfs[kvp.Key].Add(kvp.Value);
                }
            }

            return pfs
                .Where(p => p.Value.Count == _retentionPolicies.Count)
                .ToDictionary(p => p.Key, p => p.Value.Min())
                .Aggregate(1, (result, kvp) => result * (int)Math.Pow(kvp.Key, kvp.Value));
        }
    }

    /// <summary>
    /// Gets or sets the authentication scheme, used for the index page. Leave empty for no authentication.
    /// </summary>
    public string? AuthenticationScheme { get; set; }
    /// <summary>
    /// Gets the logging level switch.
    /// </summary>
    public LoggingLevelSwitch LoggingLevelSwitch { get; private set; }
    /// <summary>
    /// The list of configured retention policies
    /// </summary>
    public IReadOnlyList<RetentionPolicy> RetentionPolicies => _retentionPolicies.AsReadOnly();
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
    /// Adds a new retention policy.
    /// </summary>
    /// <param name="age">The log event age at which it should be deleted.</param>
    /// <param name="logLevels">
    /// The list of log levels to match to trigger event deletion. Leave empty to matches all log levels.
    /// </param>
    /// <returns></returns>
    public ISejilSettings AddRetentionPolicy(TimeSpan age, params LogEventLevel[] logLevels)
    {
        foreach (var rp in _retentionPolicies)
        {
            foreach (var l in rp.LogLevels)
            {
                if (logLevels.Contains(l))
                {
                    throw new InvalidOperationException($"A retention policy for log level '{l}' has already been defined.");
                }
            }
        }

        if (!logLevels.Any() && _retentionPolicies.Any(p => !p.LogLevels.Any()))
        {
            throw new InvalidOperationException("A retention policy that matches all levels has already been defined.");
        }

        if (!logLevels.Any() && _retentionPolicies.Any(p => age <= p.Age))
        {
            throw new InvalidOperationException("A non-constrained retention policy may not have a lower age than a constraint retention policy.");
        }

        if (logLevels.Any() && _retentionPolicies.Any(p => !p.LogLevels.Any() && age >= p.Age))
        {
            throw new InvalidOperationException("A constrained retention policy may not have a higher age than a non-constraint retention policy.");
        }

        _retentionPolicies.Add(new(age, logLevels));
        return this;
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

    private static Dictionary<int, int> GetPrimeFactors(int num)
    {
        Debug.Assert(num >= 2);

        var result = new List<int>();

        while (num % 2 == 0)
        {
            result.Add(2);
            num /= 2;
        }

        var factor = 3;
        while (factor * factor <= num)
        {
            if (num % factor == 0)
            {
                result.Add(factor);
                num /= factor;
            }
            else
            {
                factor += 2;
            }
        }

        if (num > 1)
        {
            result.Add(num);
        }

        return result
            .GroupBy(p => p)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
