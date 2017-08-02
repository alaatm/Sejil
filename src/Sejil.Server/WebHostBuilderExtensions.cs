using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Sejil;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// Adds Sejil services.
        /// </summary>
        /// <param name="builder">The web host builder.</param>
        /// <param name="url">The URL at which Sejil should be available.</param>
        /// <param name="minLogLevel">The minimum log level.</param>
        /// <returns></returns>
        public static IWebHostBuilder AddSejil(this IWebHostBuilder builder, string url, LogLevel minLogLevel)
        {
            var settings = new SejilSettings(url, MapSerilogLogLevel(minLogLevel));

            return builder
#if NETSTANDARD1_6
                .ConfigureLogging((logging) => logging.AddSerilog(CreateLogger(settings)))
#elif NETSTANDARD2_0
                .ConfigureLogging((_, logging) => logging.AddSerilog(CreateLogger(settings)))
#endif
                .ConfigureServices(services => 
                {
                    services.AddSingleton(settings);
                    services.AddScoped<ISejilRepository, SejilRepository>();
                    services.AddScoped<ISejilSqlProvider, SejilSqlProvider>();
                    services.AddScoped<ISejilController, SejilController>();
                });
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