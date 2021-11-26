using Serilog.Events;

namespace Sejil.Configuration;

public class RetentionPolicy
{
    /// <summary>
    /// The log event age at which it should be deleted.
    /// </summary>
    public TimeSpan Age { get; }
    /// <summary>
    /// The list of log levels to match to trigger event deletion.
    /// </summary>
    public IReadOnlyList<LogEventLevel> LogLevels { get; }

    internal RetentionPolicy(TimeSpan age, IEnumerable<LogEventLevel> logLevels)
    {
        if (age < TimeSpan.FromMinutes(2))
        {
            throw new InvalidOperationException("The lowest possible age is 2 minutes.");
        }

        if (age.Seconds != 0 || age.Milliseconds != 0)
        {
            throw new InvalidOperationException("The age cannot have 'seconds' or 'milli-seconds' components.");
        }

        Age = age;
        LogLevels = logLevels.ToList();
    }
}
