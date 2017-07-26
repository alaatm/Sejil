using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.MSSqlServer;
using LogsExplorer.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting
{
    public static class IWebHostBuilderExtensions
    {
        public static IWebHostBuilder UseLogsExplorer(this IWebHostBuilder builder, string connStr, string tableName, string propertiesTableName)
            => builder
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddSerilog(CreateSerilogLogger(connStr, tableName, propertiesTableName));
                })
                .ConfigureServices(services =>
                    services.AddSingleton<ILogsExplorerOptions>(new LogsExplorerOptions(connStr, tableName, propertiesTableName)));

        private static Serilog.Core.Logger CreateSerilogLogger(string connStr, string tableName, string propertiesTableName)
            => new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.MSSqlServer(connStr, tableName, propertiesTableName, autoCreateSqlTable: true)
                .CreateLogger();

        private static ILoggingBuilder AddSerilog(this ILoggingBuilder logging, Serilog.Core.Logger logger = null, bool dispose = false)
            => logging.AddProvider(new SerilogLoggerProvider(logger, dispose));
    }
}
