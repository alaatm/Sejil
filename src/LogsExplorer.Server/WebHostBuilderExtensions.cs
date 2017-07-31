using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using LogsExplorer.Server;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder AddLogsExplorer(this IWebHostBuilder builder, string uri, LogLevel minLogLevel)
        {
            var settings = new LogsExplorerSettings(uri, MapSerilogLogLevel(minLogLevel));

            return builder
                .ConfigureLogging((_, logging) => logging.AddSerilog(CreateLogger(settings)))
                .ConfigureServices(services => services.AddSingleton(settings));
        }

        private static Serilog.Core.Logger CreateLogger(LogsExplorerSettings settings)
            => new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.ControlledBy(settings.LoggingLevelSwitch)
                .WriteTo.LogsExplorer(settings)
                .CreateLogger();

        private static LogEventLevel MapSerilogLogLevel(LogLevel logLevel)
        {
            if (logLevel == LogLevel.None)
            {
                throw new InvalidOperationException("Minimum log level cannot be set to None.");
            }

            return (LogEventLevel)((int)logLevel);
        }
    }
}