// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Sejil.Configuration;
using Serilog.Events;

namespace Sejil.Test.Configuration;

public class SejilSettingsTests
{
    [Fact]
    public void Ctor_loads_app_html()
    {
        // Arrange & act
        var settings = new SejilSettings("url", LogEventLevel.Debug);

        // Assert
        Assert.Equal(ResourceHelper.GetEmbeddedResource(typeof(SejilSettings).Assembly, "Sejil.index.html"), settings.SejilAppHtml);
    }

    [Fact]
    public void Ctor_save_and_adds_a_leading_slash_to_specified_url_when_missing()
    {
        // Arrange & act
        var settings = new SejilSettings("url", LogEventLevel.Debug);

        // Assert
        Assert.Equal("/url", settings.Url);
    }

    [Fact]
    public void Ctor_saves_specified_url()
    {
        // Arrange & act
        var settings = new SejilSettings("/url", LogEventLevel.Debug);

        // Assert
        Assert.Equal("/url", settings.Url);
    }

    [Fact]
    public void Ctor_sets_inital_min_log_level()
    {
        // Arrange & act
        var initalLogLevel = LogEventLevel.Debug;
        var settings = new SejilSettings("/url", initalLogLevel);

        // Assert
        Assert.Equal(initalLogLevel, settings.LoggingLevelSwitch.MinimumLevel);
    }

    [Fact]
    public void Ctor_sets_default_settings()
    {
        // Arrange & act
        var settings = new SejilSettings("", LogEventLevel.Debug);

        // Assert
        Assert.Equal(100, settings.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SetPage_throws_when_out_of_range(int pageSize)
    {
        // Arrange
        var settings = new SejilSettings("", LogEventLevel.Debug);

        // Act & assert
        Assert.Throws<ArgumentOutOfRangeException>(() => settings.PageSize = pageSize);
    }

    [Theory]
    [InlineData("Trace", LogEventLevel.Verbose, true)]
    [InlineData("verbose", LogEventLevel.Verbose, true)]
    [InlineData("DEBUG", LogEventLevel.Debug, true)]
    [InlineData("information", LogEventLevel.Information, true)]
    [InlineData("Warning", LogEventLevel.Warning, true)]
    [InlineData("Error", LogEventLevel.Error, true)]
    [InlineData("Critical", LogEventLevel.Fatal, true)]
    [InlineData("faTAL", LogEventLevel.Fatal, true)]
    [InlineData("none", (LogEventLevel)0, false)]
    public void TrySetMinimumLogLevel_attempts_to_sets_specified_min_log_level(string logLevel, LogEventLevel expected, bool expectedResult)
    {
        // Arrange
        var initialLogLevel = LogEventLevel.Debug;
        var settings = new SejilSettings("", initialLogLevel);

        // Act
        var result = settings.TrySetMinimumLogLevel(logLevel);

        // Assert
        Assert.Equal(expectedResult, result);
        if (result)
        {
            Assert.Equal(expected, settings.LoggingLevelSwitch.MinimumLevel);
        }
        else
        {
            Assert.Equal(initialLogLevel, settings.LoggingLevelSwitch.MinimumLevel);
        }
    }

    [Fact]
    public void MinimumSchedulerTimerInMinutes_returns_min_scheduler_timer_in_minutes()
    {
        // Arrange
        var settings = new SejilSettings("", default)
            .AddRetentionPolicy(TimeSpan.FromHours(12), LogEventLevel.Debug)
            .AddRetentionPolicy(TimeSpan.FromHours(50), LogEventLevel.Information)
            .AddRetentionPolicy(TimeSpan.FromHours(45.25), LogEventLevel.Warning)
            .AddRetentionPolicy(TimeSpan.FromDays(5), LogEventLevel.Error)
            .AddRetentionPolicy(TimeSpan.FromDays(30), LogEventLevel.Fatal);

        // Act
        var result = settings.MinimumSchedulerTimerInMinutes;

        // Assert
        Assert.Equal(15, result);
    }

    [Fact]
    public void MinimumSchedulerTimerInMinutes_returns_min_scheduler_timer_in_minutes2()
    {
        // Arrange
        var settings = new SejilSettings("", default)
            .AddRetentionPolicy(TimeSpan.FromMinutes(2), LogEventLevel.Debug)
            .AddRetentionPolicy(TimeSpan.FromMinutes(3), LogEventLevel.Information);

        // Act
        var result = settings.MinimumSchedulerTimerInMinutes;

        // Assert
        Assert.Equal(1, result);
    }
}
