using System;
using System.Collections.Generic;

namespace Sejil
{
    public class LogEntry
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public string MessageTemplate { get; set; }
        public string Level { get; set; }
        public DateTime Timestamp { get; set; }
        public string Exception { get; set; }
        public string SourceContext { get; set; }
        public string RequestId { get; set; }
        public List<LogEntryProperty> Properties { get; set; }
    }
}
