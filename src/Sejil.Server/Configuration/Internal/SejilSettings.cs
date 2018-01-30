// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Serilog.Events;
using Serilog.Core;
using System.IO;
using System;
#if NETSTANDARD1_6
using System.Runtime.InteropServices;
#endif

namespace Sejil.Configuration.Internal
{
    public class SejilSettings : ISejilSettings
    {
        private const string UUID = "59A8F730-6AC5-427A-9492-A3A9EAD9556F";

        public string SejilAppHtml { get; private set; }
        public string Url { get; private set; }
        public LoggingLevelSwitch LoggingLevelSwitch { get; private set; }
        public string SqliteDbPath { get; private set; }
        public string[] NonPropertyColumns { get; private set; }
        public int PageSize { get; private set; }

        public SejilSettings(string uri, LogEventLevel minLogLevel)
        {
            SejilAppHtml = ResourceHelper.GetEmbeddedResource("Sejil.index.html");
            Url = uri.StartsWith("/") ? uri : "/" + uri;
            LoggingLevelSwitch = new LoggingLevelSwitch
            {
                MinimumLevel = minLogLevel
            };

#if NETSTANDARD1_6
            var appDataFolder = Environment.GetEnvironmentVariable(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "LocalAppData" : "Home");
#elif NETSTANDARD2_0
            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
#endif

            var appName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            SqliteDbPath = Path.Combine(appDataFolder, appName, $"Sejil-{UUID}.sqlite");

            NonPropertyColumns = new[] { "message", "messageTemplate", "level", "timestamp", "exception" };
            PageSize = 100;
        }

        public bool TrySetMinimumLogLevel(string minLogLevel)
        {
            switch (minLogLevel.ToLower())
            {
                case "trace":
                    LoggingLevelSwitch.MinimumLevel = LogEventLevel.Verbose;
                    return true;
                case "debug":
                    LoggingLevelSwitch.MinimumLevel = LogEventLevel.Debug;
                    return true;
                case "information":
                    LoggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
                    return true;
                case "warning":
                    LoggingLevelSwitch.MinimumLevel = LogEventLevel.Warning;
                    return true;
                case "error":
                    LoggingLevelSwitch.MinimumLevel = LogEventLevel.Error;
                    return true;
                case "critical":
                    LoggingLevelSwitch.MinimumLevel = LogEventLevel.Fatal;
                    return true;
            }

            return false;
        }
    }
}