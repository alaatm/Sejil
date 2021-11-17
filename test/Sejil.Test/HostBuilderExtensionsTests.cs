// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sejil.Configuration.Internal;
using Sejil.Routing.Internal;
using Serilog.Events;
using Microsoft.AspNetCore.Hosting.Server;
using Sejil.Data.Internal;
using Microsoft.Extensions.Hosting;
using Serilog.Core;

namespace Sejil.Test
{
    public class HostBuilderExtensionsTests
    {
        [Theory]
        [InlineData(LogLevel.Trace, LogEventLevel.Verbose)]
        [InlineData(LogLevel.Debug, LogEventLevel.Debug)]
        [InlineData(LogLevel.Information, LogEventLevel.Information)]
        [InlineData(LogLevel.Warning, LogEventLevel.Warning)]
        [InlineData(LogLevel.Error, LogEventLevel.Error)]
        [InlineData(LogLevel.Critical, LogEventLevel.Fatal)]
        public void UseSejil_sets_intial_settings(LogLevel logLevel, LogEventLevel expectedMappedLogLevel)
        {
            // Arrange
            var url = "/sejil";
            var hostBuilder = new HostBuilder()
                .ConfigureServices(services => services.AddSingleton(Mock.Of<IServer>()));

            // Act
            hostBuilder.UseSejil(minLogLevel: logLevel);

            // Assert
            var settings = hostBuilder.Build().Services.GetRequiredService<ISejilSettings>();
            Assert.Equal(url, settings.Url);
            Assert.Equal(expectedMappedLogLevel, settings.LoggingLevelSwitch.MinimumLevel);
        }

        [Fact]
        public void UseSejil_registeres_required_services()
        {
            // Arrange
            var hostBuilder = new HostBuilder()
                .ConfigureServices(services =>
                    services.AddSingleton(Mock.Of<IServer>()));

            // Act
            hostBuilder.UseSejil();

            // Assert
            var webhost = hostBuilder.Build();

            using var scope = webhost.Services.CreateScope();
            var settings = scope.ServiceProvider.GetService(typeof(ISejilSettings));
            var repository = scope.ServiceProvider.GetService(typeof(ISejilRepository));
            var sqlProvider = scope.ServiceProvider.GetService(typeof(ISejilSqlProvider));
            var controller = scope.ServiceProvider.GetService(typeof(ISejilController));

            Assert.NotNull(settings);
            Assert.NotNull(repository);
            Assert.NotNull(sqlProvider);
            Assert.NotNull(controller);
        }

        [Fact]
        public void UseSejil_throws_when_none_is_passed_for_log_level()
        {
            // Arrange
            var hostBuilder = new HostBuilder()
                .ConfigureServices(services => services.AddSingleton(Mock.Of<IServer>()));

            // Act & assert
            var ex = Assert.Throws<InvalidOperationException>(() => hostBuilder.UseSejil(minLogLevel: LogLevel.None));
            Assert.Equal("Minimum log level cannot be set to None.", ex.Message);
        }

        [Fact]
        public void UseSejil_can_write_to_providers()
        {
            // Arrange
            var testLoggerProvider = new TestLoggerProvider();
            var hostBuilder = new HostBuilder()
                .ConfigureLogging(logging => logging.AddProvider(testLoggerProvider))
                .ConfigureServices(services =>
                    services.AddSingleton(Mock.Of<IServer>()));

            Assert.False(testLoggerProvider.TestLogger.DidLog);

            // Act
            hostBuilder.UseSejil(writeToProviders: true);

            var webhost = hostBuilder.Build();
            var f = webhost.Services.GetService<ILoggerFactory>();
            f.CreateLogger("test").LogInformation("Hello, world!");

            // Assert
            Assert.True(testLoggerProvider.TestLogger.DidLog);
        }

        [Fact]
        public void UseSejil_can_register_additional_sinks()
        {
            // Arrange
            var hostBuilder = new HostBuilder()
                .ConfigureServices(services =>
                    services.AddSingleton(Mock.Of<IServer>()));

            var sink = new TestSink();
            Assert.Empty(sink.Writes);

            // Act
            hostBuilder.UseSejil(sinks: sinks => sinks.Sink(sink));

            var webhost = hostBuilder.Build();
            var f = webhost.Services.GetService<ILoggerFactory>();
            f.CreateLogger("test").LogInformation("Hello, world!");

            // Assert
            var evt = Assert.Single(sink.Writes);
            Assert.Equal("Hello, world!", evt.MessageTemplate.Text);
        }

        class TestLogger : ILogger
        {
            public bool DidLog { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();
            public bool IsEnabled(LogLevel logLevel) => throw new NotImplementedException();
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) => DidLog = true;
        }

        class TestLoggerProvider : ILoggerProvider
        {
            public TestLogger TestLogger { get; } = new TestLogger();
            public ILogger CreateLogger(string categoryName) => TestLogger;
            public void Dispose() { }
        }

        class TestSink : ILogEventSink
        {
            public List<LogEvent> Writes { get; set; } = new List<LogEvent>();

            public void Emit(LogEvent logEvent) => Writes.Add(logEvent);
        }
    }
}
