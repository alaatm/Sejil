using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using System.IO;

namespace LogsExplorer.Server
{
    public class LogsExplorerSettings
    {
        private const string UUID = "59A8F730-6AC5-427A-9492-A3A9EAD9556F";

        public string Uri { get; private set; }
        public LoggingLevelSwitch LoggingLevelSwitch { get; private set; }
        public string SqliteDbPath { get; private set; }

        public LogsExplorerSettings(string uri, LogEventLevel minLogLevel)
        {
            Uri = uri.StartsWith("/") ? uri : "/" + uri;
            LoggingLevelSwitch = new LoggingLevelSwitch
            {
                MinimumLevel = minLogLevel
            };
            var basePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            SqliteDbPath = Path.Combine(Path.GetDirectoryName(basePath), $"LogsExplorer-{UUID}.sqlite");
        }
    }
}