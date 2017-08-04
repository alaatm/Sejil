// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Sejil.Models.Internal
{
    public class LogEntry
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public string MessageTemplate { get; set; }
        public string Level { get; set; }
        public DateTime Timestamp { get; set; }
        public string Exception { get; set; }
        public List<LogEntryProperty> Properties { get; set; } = new List<LogEntryProperty>();
    }
}
