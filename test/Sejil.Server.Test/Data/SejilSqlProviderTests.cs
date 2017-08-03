using System;
using System.Collections.Generic;
using Moq;
using Sejil.Configuration.Internal;
using Sejil.Data.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Sejil.Test.Data
{
    public class SejilSqlProviderTests
    {
        [Fact]
        public void GetSavedQueriesSql_returns_correct_sql()
        {
            // Arrange
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act
            var sql = provider.GetSavedQueriesSql();

            // Assert
            Assert.Equal("SELECT * FROM log_query", sql);
        }

        [Fact]
        public void InsertLogQuerySql_returns_correct_sql()
        {
            // Arrange
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act
            var sql = provider.InsertLogQuerySql();

            // Assert
            Assert.Equal("INSERT INTO log_query (name, query) VALUES (@name, @query)", sql);
        }

        [Fact]
        public void GetPagedLogEntriesSql_throws_when_page_arg_is_zero()
        {
            // Arrange
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act & assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>("page", () => provider.GetPagedLogEntriesSql(0, 1, DateTime.MinValue, null));
            Assert.Equal($"Argument must be greater than zero.{Environment.NewLine}Parameter name: page", ex.Message);
        }

        [Fact]
        public void GetPagedLogEntriesSql_throws_when_page_arg_is_less_than_zero()
        {
            // Arrange
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act & assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>("page", () => provider.GetPagedLogEntriesSql(-1, 1, DateTime.MinValue, null));
            Assert.Equal($"Argument must be greater than zero.{Environment.NewLine}Parameter name: page", ex.Message);
        }

        [Fact]
        public void GetPagedLogEntriesSql_throws_when_pageSize_arg_is_zero()
        {
            // Arrange
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act & assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>("pageSize", () => provider.GetPagedLogEntriesSql(1, 0, DateTime.MinValue, null));
            Assert.Equal($"Argument must be greater than zero.{Environment.NewLine}Parameter name: pageSize", ex.Message);
        }

        [Fact]
        public void GetPagedLogEntriesSql_throws_when_pageSize_arg_is_less_than_zero()
        {
            // Arrange
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act & assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>("pageSize", () => provider.GetPagedLogEntriesSql(1, -1, DateTime.MinValue, null));
            Assert.Equal($"Argument must be greater than zero.{Environment.NewLine}Parameter name: pageSize", ex.Message);
        }

        [Fact]
        public void GetPagedLogEntriesSql_returns_correct_sql_for_page()
        {
            // Arrange
            var page = 2;
            var pageSize = 100;
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act
            var sql = provider.GetPagedLogEntriesSql(page, pageSize, DateTime.MinValue, null);

            // Assert
            Assert.Equal(
$@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    
    
    ORDER BY timestamp DESC
    LIMIT {pageSize} OFFSET {(page - 1) * pageSize}
) l
LEFT JOIN log_property p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name", sql);
        }

        [Fact]
        public void GetPagedLogEntriesSql_ignores_timestamp_arg_when_it_equals_DateTime_MinValue()
        {
            // Arrange
            var timestamp = DateTime.MinValue;
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act
            var sql = provider.GetPagedLogEntriesSql(2, 100, timestamp, null);

            // Assert
            Assert.Equal(
@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    
    
    ORDER BY timestamp DESC
    LIMIT 100 OFFSET 100
) l
LEFT JOIN log_property p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name", sql);
        }

        [Fact]
        public void GetPagedLogEntriesSql_returns_correct_sql_for_events_before_specified_timestamp()
        {
            // Arrange
            var timestamp = new DateTime(2017, 8, 3, 14, 56, 33, 876);
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act
            var sql = provider.GetPagedLogEntriesSql(2, 100, timestamp, null);

            // Assert
            Assert.Equal(
@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    WHERE timestamp <= '2017-08-03 14:56:33.876'
    
    ORDER BY timestamp DESC
    LIMIT 100 OFFSET 100
) l
LEFT JOIN log_property p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name", sql);
        }

        [Fact]
        public void GetPagedLogEntriesSql_returns_correct_sql_for_events_before_specified_timestamp_with_specified_query()
        {
            // Arrange
            var timestamp = new DateTime(2017, 8, 3, 14, 56, 33, 876);
            var query = "p=v";
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act
            var sql = provider.GetPagedLogEntriesSql(2, 100, timestamp, query);

            // Assert
            Assert.Equal(
@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    WHERE timestamp <= '2017-08-03 14:56:33.876'
    AND id IN (SELECT logId FROM log_property WHERE name = 'p' AND value = 'v')
    ORDER BY timestamp DESC
    LIMIT 100 OFFSET 100
) l
LEFT JOIN log_property p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name", sql);
        }

        [Theory]
        [MemberData(nameof(GetPagedLogEntriesSql_TestData))]
        public void GetPagedLogEntriesSql_returns_correct_sql_for_events_with_specified_query(string query, string expectedSql, params string[] nonPropertyColumns)
        {
            // Arrange
            var settingsMoq = new Mock<ISejilSettings>();
            settingsMoq.SetupGet(p => p.NonPropertyColumns).Returns(nonPropertyColumns);
            var provider = new SejilSqlProvider(settingsMoq.Object);

            // Act
            var sql = provider.GetPagedLogEntriesSql(2, 100, DateTime.MinValue, query);

            // Assert
            Assert.Equal(expectedSql, GetInnerPredicate(sql));
        }

        public static IEnumerable<object[]> GetPagedLogEntriesSql_TestData()
        {
            // ... =|!=|like|not like ...
            yield return new object[]
            {
                "prob1 = value1",
                "id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value1')"
            };
            yield return new object[]
            {
                "prob1 != 'value1'",
                "id NOT IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value1')"
            };
            yield return new object[]
            {
                "prob1 like \"value1\"",
                "id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value LIKE 'value1')"
            };
            yield return new object[]
            {
                "prob1 not like value1",
                "id NOT IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value LIKE 'value1')"
            };
            // ... and|or ...
            yield return new object[]
            {
                "prob1 = value1 && prob2 = 'value2'",
                "id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value1') AND "+
                "id IN (SELECT logId FROM log_property WHERE name = 'prob2' AND value = 'value2')"
            };
            yield return new object[]
            {
                "prob1 = value1 and prob2 != 'value2'",
                "id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value1') AND "+
                "id NOT IN (SELECT logId FROM log_property WHERE name = 'prob2' AND value = 'value2')"
            };
            yield return new object[]
            {
                "prob1 not like value1 or prob2 like 'value2'",
                "id NOT IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value LIKE 'value1') OR "+
                "id IN (SELECT logId FROM log_property WHERE name = 'prob2' AND value LIKE 'value2')"
            };
            yield return new object[]
            {
                "prob1 not like value1 || prob2 not like 'value2'",
                "id NOT IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value LIKE 'value1') OR "+
                "id NOT IN (SELECT logId FROM log_property WHERE name = 'prob2' AND value LIKE 'value2')"
            };
            // (...) and|or ...
            yield return new object[]
            {
                "(prob1 = value1 || prob1 = 'value2') && prob3 like value3",
                "(id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value1') OR "+
                "id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value2')) AND "+
                "id IN (SELECT logId FROM log_property WHERE name = 'prob3' AND value LIKE 'value3')"
            };
            yield return new object[]
            {
                "(prob1 = value1 && prob2 = 'value2') || prob3 not like value3",
                "(id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value1') AND "+
                "id IN (SELECT logId FROM log_property WHERE name = 'prob2' AND value = 'value2')) OR "+
                "id NOT IN (SELECT logId FROM log_property WHERE name = 'prob3' AND value LIKE 'value3')"
            };
            // ... and|or (...)
            yield return new object[]
            {
                "prob1 = value1 || (prob1 = 'value2' && prob3 like value3)",
                "id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value1') OR "+
                "(id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value2') AND "+
                "id IN (SELECT logId FROM log_property WHERE name = 'prob3' AND value LIKE 'value3'))"
            };
            yield return new object[]
            {
                "prob1 = value1 && (prob2 = 'value2' || prob3 not like value3)",
                "id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value1') AND "+
                "(id IN (SELECT logId FROM log_property WHERE name = 'prob2' AND value = 'value2') OR "+
                "id NOT IN (SELECT logId FROM log_property WHERE name = 'prob3' AND value LIKE 'value3'))"
            };
            // (...) and|or (...)
            yield return new object[]
            {
                "(prob1 = value1 || prob1 = 'value2') && (prob3 like value3 && prob4 != 5)",
                "(id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value1') OR "+
                "id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value2')) AND "+
                "(id IN (SELECT logId FROM log_property WHERE name = 'prob3' AND value LIKE 'value3') AND " +
                "id NOT IN (SELECT logId FROM log_property WHERE name = 'prob4' AND value = '5'))"
            };
            yield return new object[]
            {
                "(prob1 = value1 || prob1 = 'value2') || (prob3 like value3 && prob4 != 5)",
                "(id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value1') OR "+
                "id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value2')) OR "+
                "(id IN (SELECT logId FROM log_property WHERE name = 'prob3' AND value LIKE 'value3') AND " +
                "id NOT IN (SELECT logId FROM log_property WHERE name = 'prob4' AND value = '5'))"
            };
            // Non property columns
            yield return new object[]
            {
                "SourceContext like '%ClassName%'",
                "SourceContext LIKE '%ClassName%'",
                "sourcecontext"
            };
            yield return new object[]
            {
                "prob1 = value1 || SourceContext like '%ClassName%' && prob2 != value2",
                "id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value1') OR " +
                "SourceContext LIKE '%ClassName%' AND " +
                "id NOT IN (SELECT logId FROM log_property WHERE name = 'prob2' AND value = 'value2')",
                "sourcecontext"
            };
            // General text
            yield return new object[]
            {
                "search",
                "(message LIKE '%search%' OR exception LIKE '%search%' OR " +
                "id in (SELECT logId FROM log_property WHERE value LIKE '%search%'))"
            };
            yield return new object[]
            {
                "prob1 = value1 && search",
                "id IN (SELECT logId FROM log_property WHERE name = 'prob1' AND value = 'value1') AND " +
                "(message LIKE '%search%' OR exception LIKE '%search%' OR " +
                "id in (SELECT logId FROM log_property WHERE value LIKE '%search%'))"
            };
        }

        private string GetInnerPredicate(string sql)
        {
            return sql.Replace(
@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    
    WHERE ", "").Replace(
@"
    ORDER BY timestamp DESC
    LIMIT 100 OFFSET 100
) l
LEFT JOIN log_property p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name", "");
        }
    }
}