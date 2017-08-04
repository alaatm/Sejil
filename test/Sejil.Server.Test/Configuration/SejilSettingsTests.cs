// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Moq;
using Sejil.Configuration.Internal;
using Sejil.Data.Internal;
using Sejil.Models.Internal;
using Xunit;
using Xunit.Abstractions;
using Dapper;
using System.Linq;
using Serilog.Events;
using System.IO;

namespace Sejil.Test.Configuration
{
    public class SejilSettingsTests
    {
        [Fact]
        public void Ctor_loads_app_html()
        {
            // Arrange & act
            var settings = new SejilSettings("url", LogEventLevel.Debug);

            // Assert
            Assert.Equal(ResourceHelper.GetEmbeddedResource("Sejil.index.html"), settings.SejilAppHtml);
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
        public void Ctor_sets_db_path_to_entry_assembly_path()
        {
            // Arrange & act
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var settings = new SejilSettings("", LogEventLevel.Debug);

            // Assert
            Assert.Equal(basePath, Path.GetDirectoryName(settings.SqliteDbPath));
        }

        [Fact]
        public void Ctor_sets_default_db_name()
        {
            // Arrange & act
            var settings = new SejilSettings("", LogEventLevel.Debug);

            // Assert
            Assert.Matches(@"^Sejil-[0-9A-F]{8}-([0-9A-F]{4}-){3}[0-9A-F]{12}\.sqlite$", Path.GetFileName(settings.SqliteDbPath));
        }

        [Fact]
        public void Ctor_sets_default_settings()
        {
            // Arrange & act
            var settings = new SejilSettings("", LogEventLevel.Debug);

            // Assert
            Assert.Equal(new[] { "message", "messageTemplate", "level", "timestamp", "exception" },
                settings.NonPropertyColumns);
            Assert.Equal(100, settings.PageSize);
        }

        [Theory]
        [InlineData("Trace", LogEventLevel.Verbose, true)]
        [InlineData("DEBUG", LogEventLevel.Debug, true)]
        [InlineData("information", LogEventLevel.Information, true)]
        [InlineData("Warning", LogEventLevel.Warning, true)]
        [InlineData("Error", LogEventLevel.Error, true)]
        [InlineData("Critical", LogEventLevel.Fatal, true)]
        [InlineData("none", 0, false)]
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
}