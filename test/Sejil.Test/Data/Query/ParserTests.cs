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
            var ex = Assert.Throws<QueryEngineException>(() => new Parser(new Scanner("(p = 'v'))").Scan()).Parse());
            Assert.Equal("Error at position '10' -> ): Expect end of line.", ex.Message);
        }

        [Fact]
        public void Parse_throws_when_missing_closing_paranth()
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Parser(new Scanner("(p = 'v'").Scan()).Parse());
            Assert.Equal("Error at position '9': Expect ')' after expression.", ex.Message);
        }

        [Fact]
        public void Parse_throws_when_missing_expression()
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Parser(new Scanner("p =").Scan()).Parse());
            Assert.Equal("Error at position '4': Expect expression.", ex.Message);
        }

        [Fact]
        public void Parse_throws_when_left_side_of_binary_is_non_var()
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Parser(new Scanner("'v'=p").Scan()).Parse());
            Assert.Equal("Error at position '1' -> 'v': Expect identifier.", ex.Message);
        }

        [Fact]
        public void Parse_throws_when_right_side_of_binary_is_non_literal()
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Parser(new Scanner("p=p").Scan()).Parse());
            Assert.Equal("Error at position '3' -> p: Expect literal.", ex.Message);
        }

        [Theory]
        [InlineData("p like 5", 8)]
        [InlineData("p not like 5", 12)]
        public void Parse_throws_when_like_notLike_op_used_with_non_string_value(string source, int pos)
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Parser(new Scanner(source).Scan()).Parse());
            Assert.Equal($"Error at position '{pos}' -> 5: Expect string literal.", ex.Message);
        }

        [Theory]
        [InlineData("p > '5'", 5)]
        [InlineData("p >= '5'", 6)]
        [InlineData("p < '5'", 5)]
        [InlineData("p <= '5'", 6)]
        public void Parse_throws_when_gt_gte_lt_lte_op_used_with_non_numeric_value(string source, int pos)
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Parser(new Scanner(source).Scan()).Parse());
            Assert.Equal($"Error at position '{pos}' -> '5': Expect numeric literal.", ex.Message);
        }

        [Theory]
        [InlineData("l and x = 5", 1)]
        [InlineData("l or x = 5", 1)]
        public void Parse_throws_when_left_side_of_logical_is_not_binary_grouping_or_logical(string source, int pos)
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Parser(new Scanner(source).Scan()).Parse());
            Assert.Equal($"Error at position '{pos}' -> l: Expect binary, grouping or logical expression.", ex.Message);
        }

        [Theory]
        [InlineData("x = 5 and r", 11)]
        [InlineData("x = 5 or r", 10)]
        public void Parse_throws_when_right_side_of_logical_is_not_binary_grouping_or_logical(string source, int pos)
        {
            var ex = Assert.Throws<QueryEngineException>(() => new Parser(new Scanner(source).Scan()).Parse());
            Assert.Equal($"Error at position '{pos}' -> r: Expect binary, grouping or logical expression.", ex.Message);
        }
    }
}
