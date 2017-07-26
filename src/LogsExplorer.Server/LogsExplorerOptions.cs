using System;
using System.Collections.Generic;
using System.Text;

namespace LogsExplorer.Server
{
    public interface ILogsExplorerOptions
    {
        string ConnectionString { get; }
        string LogsTableName { get; }
        string LogsPropertiesTableName { get; }
    }

    public class LogsExplorerOptions : ILogsExplorerOptions
    {
        public string ConnectionString { get; }
        public string LogsTableName { get; }
        public string LogsPropertiesTableName { get; }

        public LogsExplorerOptions(string connectionString, string logsTableName, string logsPropertiesTableName)
        {
            ConnectionString = connectionString;
            LogsTableName = logsTableName;
            LogsPropertiesTableName = logsPropertiesTableName;
        }
    }
}
