using System;

namespace Sejil.Models.Internal
{
    public class LogEntryProperty
    {
        public int Id { get; set; }
        public Guid LogId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
