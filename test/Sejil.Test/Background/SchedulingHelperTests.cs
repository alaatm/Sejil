using Sejil.Background;
using Sejil.Configuration;
using Serilog.Events;

namespace Sejil.Test.Background;

public class SchedulingHelperTests
{
    [Theory]
    [MemberData(nameof(TestData))]
    public void GetTimerDuration_returns_min_scheduler_timer_in_minutes(
        List<RetentionPolicy> retentionPolicies,
        int expectedDuration)
    {
        // Act
        var result = SchedulingHelper.GetTimerDuration(retentionPolicies);

        // Assert
        Assert.Equal(expectedDuration, result);
    }

    public static TheoryData<List<RetentionPolicy>, int> TestData() => new()
    {
        {
            new List<RetentionPolicy>(),
            -1
        },
        {
            new List<RetentionPolicy>
            {
                new(TimeSpan.FromHours(12), new[] { LogEventLevel.Debug }),
                new(TimeSpan.FromHours(50), new[] { LogEventLevel.Information }),
                new(TimeSpan.FromHours(45.25), new[] { LogEventLevel.Warning }),
                new(TimeSpan.FromDays(5), new[] { LogEventLevel.Error }),
                new(TimeSpan.FromDays(30), new[] { LogEventLevel.Fatal }),
            },
            15
        },
        {
            new List<RetentionPolicy>
            {
                new(TimeSpan.FromMinutes(2), new[] { LogEventLevel.Debug }),
                new(TimeSpan.FromMinutes(3), new[] { LogEventLevel.Information }),
            },
            1
        },
    };
}
