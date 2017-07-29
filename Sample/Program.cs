using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddSerilog(CreateSerilogLogger("logs.db"));
                })
                .UseStartup<Startup>()
                .Build();

        private static Serilog.Core.Logger CreateSerilogLogger(string sqliteDbPath)
            => new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.LogsExplorer(sqliteDbPath)
                .CreateLogger();
    }

    public static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AddSerilog(this ILoggingBuilder logging, Serilog.Core.Logger logger = null, bool dispose = false)
            => logging.AddProvider(new SerilogLoggerProvider(logger, dispose));
    }
}
