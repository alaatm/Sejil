using System;
using Sejil.Data.Query.Internal;
using Xunit;

namespace Sejil.Test.Data.Query
{
    // Currently covering non tested cases from sql provider.
    // TODO: full coverage

    public class ParserTests
    {
        [Fact]
        public void Parse_throws_when_extra_paranth()
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Parser(new Scanner("(p = 'v'))").Scan(), Array.Empty<string>()).Parse());
            Assert.Equal("Error at position '10' ')': Expect end of line.", ex.Message);
        }

        [Fact]
        public void Parse_throws_when_missing_closing_paranth()
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Parser(new Scanner("(p = 'v'").Scan(), Array.Empty<string>()).Parse());
            Assert.Equal("Error at position '9': Expect ')' after expression.", ex.Message);
        }

        [Fact]
        public void Parse_throws_when_missing_expression()
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Parser(new Scanner("p =").Scan(), Array.Empty<string>()).Parse());
            Assert.Equal("Error at position '4': Expect expression.", ex.Message);
        }
    }
}
