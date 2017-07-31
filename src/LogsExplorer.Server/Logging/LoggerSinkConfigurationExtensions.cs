namespace Serilog
{
    using System;
    using System.IO;
    using LogsExplorer.Server;
    using LogsExplorer.Server.Logging.Sinks;
    using Serilog.Configuration;
    using Serilog.Debugging;
    using Serilog.Events;

    /// <summary>
    ///     Adds the WriteTo.LogsExplorer() extension method to <see cref="LoggerConfiguration" />.
    /// </summary>
    internal static class LoggerSinkConfigurationExtensions
    {
        /// <summary>
        ///     Adds a sink that writes log events to a SQLite database.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="sqliteDbPath">The path of SQLite db.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration LogsExplorer(
            this LoggerSinkConfiguration loggerConfiguration,
            LogsExplorerSettings settings,
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