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

    [Fact]
    public void AddRetentionPolicy_throws_when_setting_multiple_policies_for_same_logLevel()
    {
        // Arrange
        var settings = new SejilSettings("", LogEventLevel.Debug);
        settings.AddRetentionPolicy(TimeSpan.FromMinutes(2), LogEventLevel.Information);

        // Act & assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            settings.AddRetentionPolicy(TimeSpan.FromMinutes(2), LogEventLevel.Information));
        Assert.Equal("A retention policy for log level 'Information' has already been defined.", ex.Message);
    }

    [Fact]
    public void AddRetentionPolicy_throws_when_setting_multiple_policies_for_all_levels()
    {
        // Arrange
        var settings = new SejilSettings("", LogEventLevel.Debug);
        settings.AddRetentionPolicy(TimeSpan.FromMinutes(2));

        // Act & assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            settings.AddRetentionPolicy(TimeSpan.FromMinutes(2)));
        Assert.Equal("A retention policy that matches all levels has already been defined.", ex.Message);
    }

    [Fact]
    public void AddRetentionPolicy_throws_when_generic_policies_have_lower_age_than_specific()
    {
        // Arrange
        var settings = new SejilSettings("", LogEventLevel.Debug);
        settings.AddRetentionPolicy(TimeSpan.FromMinutes(5), LogEventLevel.Information);

        // Act & assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            settings.AddRetentionPolicy(TimeSpan.FromMinutes(2)));
        Assert.Equal("A non-constrained retention policy may not have a lower age than a constraint retention policy.", ex.Message);
    }

    [Fact]
    public void AddRetentionPolicy_throws_when_specific_policies_have_higher_age_than_generic()
    {
        // Arrange
        var settings = new SejilSettings("", LogEventLevel.Debug);
        settings.AddRetentionPolicy(TimeSpan.FromMinutes(2));

        // Act & assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            settings.AddRetentionPolicy(TimeSpan.FromMinutes(5), LogEventLevel.Information));
        Assert.Equal("A constrained retention policy may not have a higher age than a non-constraint retention policy.", ex.Message);
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
}
