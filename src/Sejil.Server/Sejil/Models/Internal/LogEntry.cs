// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;

namespace Sejil.Models.Internal;

public sealed class LogEntry
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

    private List<TextSpan> ExtractSpans()
    {
        var spans = new List<TextSpan>();
        var current = 0;

        foreach (Match m in _propRegex.Matches(MessageTemplate))
        {
            var prop = m.Groups[0].Value;
            var name = m.Groups[1].Value;
            name = name.Contains(':') ? name[..name.IndexOf(':')] : name;

            var value = Properties.FirstOrDefault(p => p.Name == name)?.Value;

            var startIdx = MessageTemplate.IndexOf(prop, current, StringComparison.OrdinalIgnoreCase);
            var endIdx = startIdx + prop.Length;
            var section = MessageTemplate[current..startIdx];

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
            Text = MessageTemplate[current..],
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
