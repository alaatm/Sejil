// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Reflection;
using Sejil.Configuration.Internal;
using Sejil.Logging;
using Serilog;

namespace Sejil.Test.Logging
{
    public class LoggerConfigurationExtensionsTests
    {
        [Fact]
        public void Sejil_creates_directory_as_returned_from_settings()
        {
            // Arrange
            var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Guid.NewGuid().ToString());
            var settingsMoq = new Mock<ISejilSettings>();
            settingsMoq.SetupGet(p => p.SqliteDbPath).Returns(Path.Combine(dir, "db.sqlite"));

            // Act
            Assert.False(Directory.Exists(dir));
            new LoggerConfiguration().WriteTo.Sejil(settingsMoq.Object);

            // Assert
            Assert.True(Directory.Exists(dir));
        }

        [Fact]
        public void Sejil_rethrows_any_caught_exceptions()
        {
            // Arrange
            var dir = "/!@#$/><:'+_";
            var settingsMoq = new Mock<ISejilSettings>();
            settingsMoq.SetupGet(p => p.SqliteDbPath).Returns(Path.Combine(dir, "db.sqlite"));

            // Act & assert
            Assert.ThrowsAny<Exception>(() => new LoggerConfiguration().WriteTo.Sejil(settingsMoq.Object));
        }
    }
}