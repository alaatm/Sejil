using Sejil.Models.Internal;

namespace Sejil.Test.Models;

public class LogEntryTests
{
    [Theory]
    [MemberData(nameof(Spans_returns_indivisual_fragments_TestData))]
    public void Spans_returns_indivisual_fragments(string template, LogEntryProperty[] properties, TextSpan[] expected)
    {
        // Arrange
        var log = new LogEntry
        {
            MessageTemplate = template,
            Properties = properties.ToList(),
        };

        // Act
        var spans = log.Spans.ToList();

        // Assert
        Assert.Equal(expected.Length, spans.Count);
        for (var i = 0; i < spans.Count; i++)
        {
            Assert.Equal(expected[i].Kind, spans[i].Kind);
            Assert.Equal(expected[i].Text, spans[i].Text);
        }
    }

    public static IEnumerable<object[]> Spans_returns_indivisual_fragments_TestData()
    {
        yield return new object[]
        {
                "Executed {Action} with status code {StatusCode}.",
                new []
                {
                    new LogEntryProperty { Name = "Action", Value = "Index" },
                    new LogEntryProperty { Name = "StatusCode", Value = "200" }
                },
                new []
                {
                    new TextSpan { Text = "Executed " },
                    new TextSpan { Text = "Index", Kind = "str" },
                    new TextSpan { Text = " with status code " },
                    new TextSpan { Text = "200", Kind = "num" },
                    new TextSpan { Text = "." },
                }
        };

        yield return new object[]
        {
                "Executed {Action} {{with}}} status code {StatusCode}.",
                new []
                {
                    new LogEntryProperty { Name = "Action", Value = "Index" },
                    new LogEntryProperty { Name = "StatusCode", Value = "200" }
                },
                new []
                {
                    new TextSpan { Text = "Executed " },
                    new TextSpan { Text = "Index", Kind = "str" },
                    new TextSpan { Text = " {" },
                    new TextSpan { Text = "{with}" },
                    new TextSpan { Text = "}} status code " },
                    new TextSpan { Text = "200", Kind = "num" },
                    new TextSpan { Text = "." },
                }
        };

        yield return new object[]
        {
                "{FormattedMessage:l}",
                new []
                {
                    new LogEntryProperty { Name = "FormattedMessage", Value = "Some message" },
                },
                new []
                {
                    new TextSpan { Text = "Some message", Kind = "str" },
                }
        };
    }
}
