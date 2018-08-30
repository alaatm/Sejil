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
    // Run sequentially so that we can set then remove env. variables.
    [Collection("Sequential")]
    public class SejilSettingsEnvDependantTests
    {
        [Fact]
        public void Ctor_sets_db_path_to_localAppData_when_running_outside_Azure()
        {
            // Arrange & act
            var basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "testhost");
            var settings = new SejilSettings("", LogEventLevel.Debug);

            // Assert
            Assert.Equal(basePath, Path.GetDirectoryName(settings.SqliteDbPath));
        }

        [Fact]
        public void Ctor_sets_db_path_to_home_folder_when_running_inside_Azure()
        {
            // Arrange & act
            // Azure websites will have this env. variable
            var basePath = Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory), "home");
            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", "value", EnvironmentVariableTarget.Process);
            var settings = new SejilSettings("", LogEventLevel.Debug);

            // Assert
            Assert.Equal(basePath, Path.GetDirectoryName(settings.SqliteDbPath));
            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", String.Empty, EnvironmentVariableTarget.Process);
        }
    }
}