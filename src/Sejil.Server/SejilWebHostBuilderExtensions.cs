using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Sejil;

namespace Microsoft.AspNetCore.Hosting
{
    public static class SejilWebHostBuilderExtensions
    {
        public static IWebHostBuilder AddSejil(this IWebHostBuilder builder, string uri, LogLevel minLogLevel)
        {
            var settings = new SejilSettings(uri, MapSerilogLogLevel(minLogLevel));

            return builder
            #if NETSTANDARD16
                .ConfigureLogging((logging) => logging.AddSerilog(CreateLogger(settings)))
            #elif NETSTANDARD20
                .ConfigureLogging((_, logging) => logging.AddSerilog(CreateLogger(settings)))
            #endif
                .ConfigureServices(services => services.AddSingleton(settings));
        }

        private static Serilog.Core.Logger CreateLogger(SejilSettings settings)
            => new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.ControlledBy(settings.LoggingLevelSwitch)
                .WriteTo.Sejil(settings)
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