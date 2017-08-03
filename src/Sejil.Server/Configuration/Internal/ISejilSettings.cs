using Serilog.Events;
using Serilog.Core;
using System.IO;

namespace Sejil.Configuration.Internal
{
    public interface ISejilSettings
    {
        string Url { get; }
        LoggingLevelSwitch LoggingLevelSwitch { get; }
        string SqliteDbPath { get; }
        string[] NonPropertyColumns { get; }
        int PageSize { get; }
        bool TrySetMinimumLogLevel(string minLogLevel);
    }
}