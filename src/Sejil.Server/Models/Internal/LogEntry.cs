// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sejil.Models.Internal
{
    public class LogEntry
    {
        private readonly Regex _propRegex = new("{([^{][^}]+)}", RegexOptions.Compiled);

        private List<TextSpan> _spans;

        public string Id { get; set; }
        public string Message { get; set; }
        public string MessageTemplate { get; set; }
        public string Level { get; set; }
        public DateTime Timestamp { get; set; }
        public string Exception { get; set; }
        public List<LogEntryProperty> Properties { get; set; } = new List<LogEntryProperty>();
        public List<TextSpan> Spans => _spans ??= ExtractSpans();

        List<TextSpan> ExtractSpans()
        {
            var spans = new List<TextSpan>();
            var current = 0;

            foreach (Match m in _propRegex.Matches(MessageTemplate))
            {
                var prop = m.Groups[0].Value;
                var name = m.Groups[1].Value;
                name = name.Contains(":") ? name.Substring(0, name.IndexOf(":")) : name;

                var value = Properties.FirstOrDefault(p => p.Name == name)?.Value;

                var startIdx = MessageTemplate.IndexOf(prop, current);
                var endIdx = startIdx + prop.Length;
                var section = MessageTemplate.Substring(current, startIdx - current);

                spans.Add(new TextSpan
                {
                    Text = section,
                });

                if (!string.IsNullOrEmpty(value))
                {
                    var isNumber = double.TryParse(value, out var _);
                    spans.Add(new TextSpan
                    {
                        Kind = isNumber ? "num" : "str",
                        Text = value,
                    });
                }
                else
                {
                    spans.Add(new TextSpan
                    {
                        Text = prop,
                    });
                }

                current = endIdx;
            }

            spans.Add(new TextSpan
            {
                Text = MessageTemplate.Substring(current),
            });

            var result = spans.Where(p => !string.IsNullOrEmpty(p.Text)).ToList();
            return result;
        }
    }

    public class TextSpan
    {
        public string Kind { get; set; }
        public string Text { get; set; }
    }
}
