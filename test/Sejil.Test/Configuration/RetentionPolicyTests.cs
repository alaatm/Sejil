using Sejil.Configuration;
using Serilog.Events;

namespace Sejil.Test.Configuration;

public class RetentionPolicyTests
{
    [Fact]
    public void Ctor_throws_when_age_is_less_than_2_min()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new RetentionPolicy(TimeSpan.FromMinutes(1), new List<LogEventLevel>()));
        Assert.Equal("The lowest possible age is 2 minutes.", ex.Message);
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void Ctor_throws_when_age_has_seconds_or_milliseconds_components(TimeSpan age)
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new RetentionPolicy(age, new List<LogEventLevel>()));
        Assert.Equal("The age cannot have 'seconds' or 'milli-seconds' components.", ex.Message);
    }

    public static TheoryData<TimeSpan> TestData() => new()
    {
        { TimeSpan.FromSeconds(121) },
        { TimeSpan.FromMilliseconds(120001) },
    };
}
