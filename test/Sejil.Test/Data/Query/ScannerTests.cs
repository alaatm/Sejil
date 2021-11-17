using Sejil.Data.Query.Internal;
using Xunit;

namespace Sejil.Test.Data.Query
{
    // Currently covering non tested cases from sql provider.
    // TODO: full coverage

    public class ScannerTests
    {
        [Theory]
        [InlineData("p !", 3)]
        [InlineData("p ;", 3)]
        public void Scan_throws_when_unexpected_char_is_present(string source, int pos)
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Scanner(source).Scan());
            Assert.Equal($"Unexpected character at position '{pos}'.", ex.Message);
        }

        [Theory]
        [InlineData("p = 'unterminated", 18)]
        [InlineData("p = 'unes'caped'", 17)]
        [InlineData("p = 'unes'''caped'", 19)]
        [InlineData("p = 'unes'''''caped'", 21)]
        [InlineData("p = 'unes''''c'aped'", 21)]
        [InlineData("p = 'unes''''c'''aped'", 23)]
        [InlineData("p = '''", 8)]
        [InlineData("p = '''''", 10)]
        public void Scan_throws_when_unterminated_string(string input, int pos)
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Scanner(input).Scan());
            Assert.Equal($"Unterminated string at position '{pos}'.", ex.Message);
        }

        [Theory]
        [InlineData("p = 'string'", "'string'")]
        [InlineData("p = 'es''caped'", "'es''caped'")]
        [InlineData("p = 'es''''caped'", "'es''''caped'")]
        [InlineData("p = 'es''''c''aped'", "'es''''c''aped'")]
        [InlineData("p = ''''", "''''")]
        [InlineData("p = ''''''", "''''''")]
        public void Scan_scans_strings(string input, string expectedValue)
        {
            var tokens = new Scanner(input).Scan();
            Assert.Equal(TokenType.String, tokens[2].Type);
            Assert.Equal(expectedValue, tokens[2].Text);
        }

        [Fact]
        public void Scan_throws_when_using_not_keyword_without_like_keyword()
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Scanner("p not ?").Scan());
            Assert.Equal("Unexpected character at position '7': \"not\" keyword may only be used with \"like\" keyword.", ex.Message);
        }
    }
}
