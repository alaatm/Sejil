namespace Serilog
{
    using System;
    using System.IO;
    using LogsExplorer.Server.Logging.Sinks;
    using Serilog.Configuration;
    using Serilog.Debugging;
    using Serilog.Events;

    /// <summary>
    ///     Adds the WriteTo.LogsExplorer() extension method to <see cref="LoggerConfiguration" />.
    /// </summary>
    public static class LoggerSinkConfigurationExtensions
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
            string sqliteDbPath,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            if (loggerConfiguration == null)
            {
                SelfLog.WriteLine("Logger configuration is null");
                throw new ArgumentNullException(nameof(loggerConfiguration));
            }

            if (string.IsNullOrEmpty(sqliteDbPath))
            {
                SelfLog.WriteLine("Invalid sqliteDbPath");
                throw new ArgumentNullException(nameof(sqliteDbPath));
            }

            Uri sqliteDbPathUri;
            if (!Uri.TryCreate(sqliteDbPath, UriKind.RelativeOrAbsolute, out sqliteDbPathUri))
            {
                throw new ArgumentException($"Invalid path {nameof(sqliteDbPath)}");
            }

            if (!sqliteDbPathUri.IsAbsoluteUri)
            {
                var basePath = System.Reflection.Assembly.GetEntryAssembly().Location;
                sqliteDbPath = Path.Combine(Path.GetDirectoryName(basePath), sqliteDbPath);
            }

            try
            {
                var sqliteDbFile = new FileInfo(sqliteDbPath);
                sqliteDbFile.Directory?.Create();

                return loggerConfiguration.Sink(
                    new LogsExplorerSink(sqliteDbFile.FullName),
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