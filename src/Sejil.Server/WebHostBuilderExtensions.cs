// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Sejil.Data.Internal;
using Sejil.Routing.Internal;
using Sejil.Configuration.Internal;
using Sejil.Logging;

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
                .ConfigureLogging((logging) => logging.AddSerilog(CreateLogger(settings)))
                .ConfigureServices(services => 
                {
                    services.AddSingleton<ISejilSettings>(settings);
                    services.AddScoped<ISejilRepository, SejilRepository>();
                    services.AddScoped<ISejilSqlProvider, SejilSqlProvider>();
                    services.AddScoped<ISejilController, SejilController>();
                });
        }

        private static Serilog.Core.Logger CreateLogger(ISejilSettings settings)
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