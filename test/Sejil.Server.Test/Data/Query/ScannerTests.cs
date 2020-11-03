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

        [Fact]
        public void Scan_throws_when_unterminated_string()
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Scanner("p = 'unterminated").Scan());
            Assert.Equal("Unterminated string at position '18'.", ex.Message);
        }

        [Fact]
        public void Scan_throws_when_using_not_keyword_without_like_keyword()
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Scanner("p not ?").Scan());
            Assert.Equal("Unexpected character at position '7': \"not\" keyword may only be used with \"like\" keyword.", ex.Message);
        }
    }
}
