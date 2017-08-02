namespace Serilog
{
    using System;
    using System.IO;
    using Sejil;
    using Sejil.Logging.Sinks;
    using Serilog.Configuration;
    using Serilog.Debugging;
    using Serilog.Events;

    internal static class LoggerConfigurationExtensions
    {
        public static LoggerConfiguration Sejil(
            this LoggerSinkConfiguration loggerConfiguration,
            SejilSettings settings,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            try
            {
                var sqliteDbFile = new FileInfo(settings.SqliteDbPath);
                sqliteDbFile.Directory?.Create();

                return loggerConfiguration.Sink(
                    new LogsExplorerSink(settings),
                    restrictedToMinimumLevel);
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine(ex.Message);
                throw;
            }
        }
    }
}