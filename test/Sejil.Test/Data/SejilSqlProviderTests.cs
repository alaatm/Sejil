// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Moq;
using Sejil.Configuration.Internal;
using Sejil.Data.Internal;
using Sejil.Models.Internal;
using Xunit;

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
        public void DeleteQuerySql_returns_correct_sql()
        {
            // Arrange
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act
            var sql = provider.DeleteQuerySql();

            // Assert
            Assert.Equal("DELETE FROM log_query WHERE name = @name", sql);
        }

        [Fact]
        public void GetPagedLogEntriesSql_throws_when_page_arg_is_zero()
        {
            // Arrange
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act & assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>("page", () => provider.GetPagedLogEntriesSql(0, 1, null, null));
            Assert.Equal($"Argument must be greater than zero. (Parameter 'page')", ex.Message);
        }

        [Fact]
        public void GetPagedLogEntriesSql_throws_when_page_arg_is_less_than_zero()
        {
            // Arrange
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act & assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>("page", () => provider.GetPagedLogEntriesSql(-1, 1, null, null));
            Assert.Equal($"Argument must be greater than zero. (Parameter 'page')", ex.Message);
        }

        [Fact]
        public void GetPagedLogEntriesSql_throws_when_pageSize_arg_is_zero()
        {
            // Arrange
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act & assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>("pageSize", () => provider.GetPagedLogEntriesSql(1, 0, null, null));
            Assert.Equal($"Argument must be greater than zero. (Parameter 'pageSize')", ex.Message);
        }

        [Fact]
        public void GetPagedLogEntriesSql_throws_when_pageSize_arg_is_less_than_zero()
        {
            // Arrange
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act & assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>("pageSize", () => provider.GetPagedLogEntriesSql(1, -1, null, null));
            Assert.Equal($"Argument must be greater than zero. (Parameter 'pageSize')", ex.Message);
        }

        [Fact]
        public void GetPagedLogEntriesSql_returns_correct_sql_for_page()
        {
            // Arrange
            var page = 2;
            var pageSize = 100;
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act
            var sql = provider.GetPagedLogEntriesSql(page, pageSize, null, null);

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
        public void GetPagedLogEntriesSql_ignores_timestamp_arg_when_null()
        {
            // Arrange
            DateTime? timestamp = null;
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
    WHERE (timestamp <= '2017-08-03 14:56:33.876')
    
    ORDER BY timestamp DESC
    LIMIT 100 OFFSET 100
) l
LEFT JOIN log_property p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name", sql);
        }

        [Fact]
        public void GetPagedLogEntriesSql_returns_correct_sql_for_events_before_specified_timestamp_and_with_dateFilter()
        {
            // Arrange
            var timestamp = new DateTime(2017, 8, 3, 14, 56, 33, 876);
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act
            var sql = provider.GetPagedLogEntriesSql(2, 100, timestamp, new LogQueryFilter { DateFilter = "5m" });

            // Assert
            Assert.Equal(
@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    WHERE (timestamp <= '2017-08-03 14:56:33.876' AND timestamp >= datetime('now', '-5 Minute'))
    
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
            var query = "p='v'";
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act
            var sql = provider.GetPagedLogEntriesSql(2, 100, timestamp, new LogQueryFilter { QueryText = query });

            // Assert
            Assert.Equal(
@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    WHERE (timestamp <= '2017-08-03 14:56:33.876')
    AND (id IN (SELECT logId FROM log_property GROUP BY logId HAVING SUM(name = 'p' AND value = 'v') > 0))
    ORDER BY timestamp DESC
    LIMIT 100 OFFSET 100
) l
LEFT JOIN log_property p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name", sql);
        }

        [Fact]
        public void GetPagedLogEntriesSql_returns_correct_sql_for_events_before_specified_timestamp_with_specified_filter()
        {
            // Arrange
            var timestamp = new DateTime(2017, 8, 3, 14, 56, 33, 876);
            var levelFilter = "info";
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act
            var sql = provider.GetPagedLogEntriesSql(2, 100, timestamp, new LogQueryFilter { LevelFilter = levelFilter });

            // Assert
            Assert.Equal(
@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    WHERE (timestamp <= '2017-08-03 14:56:33.876')
     AND (level = 'info')
    ORDER BY timestamp DESC
    LIMIT 100 OFFSET 100
) l
LEFT JOIN log_property p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name", sql);
        }

        [Fact]
        public void GetPagedLogEntriesSql_returns_correct_sql_for_events_with_specified_query_with_specified_filter()
        {
            // Arrange
            var query = "p='v'";
            var levelFilter = "info";
            var provider = new SejilSqlProvider(Mock.Of<ISejilSettings>());

            // Act
            var sql = provider.GetPagedLogEntriesSql(2, 100, null, new LogQueryFilter { QueryText = query, LevelFilter = levelFilter });

            // Assert
            Assert.Equal(
@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    
    WHERE (id IN (SELECT logId FROM log_property GROUP BY logId HAVING SUM(name = 'p' AND value = 'v') > 0)) AND (level = 'info')
    ORDER BY timestamp DESC
    LIMIT 100 OFFSET 100
) l
LEFT JOIN log_property p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name", sql);
        }

        [Theory]
        [MemberData(nameof(GetPagedLogEntriesSql_TestData))]
        public void GetPagedLogEntriesSql_returns_correct_sql_for_events_with_specified_query(string query, string expectedSql)
        {
            // Arrange
            var settingsMoq = new Mock<ISejilSettings>();
            var provider = new SejilSqlProvider(settingsMoq.Object);

            // Act
            var sql = provider.GetPagedLogEntriesSql(2, 100, null, new LogQueryFilter { QueryText = query });

            // Assert
            Assert.Equal(expectedSql, GetInnerPredicate(sql));
        }

        [Theory]
        [MemberData(nameof(GetPagedLogEntriesSql_returns_correct_sql_for_events_with_specified_dateFilter_TestData))]
        public void GetPagedLogEntriesSql_returns_correct_sql_for_events_with_specified_dateFilter(string dateFilter, string expectedSql)
        {
            // Arrange
            var settingsMoq = new Mock<ISejilSettings>();
            var provider = new SejilSqlProvider(settingsMoq.Object);

            // Act
            var sql = provider.GetPagedLogEntriesSql(2, 100, null, new LogQueryFilter { DateFilter = dateFilter });

            // Assert
            Assert.Equal(expectedSql, GetInnerPredicate_ts(sql));
        }

        [Theory]
        [MemberData(nameof(GetPagedLogEntriesSql_returns_correct_sql_for_events_with_specified_filter_TestData))]
        public void GetPagedLogEntriesSql_returns_correct_sql_for_events_with_specified_filter(string levelFilter, bool exceptionsOnly, string expectedSql)
        {
            // Arrange
            var settingsMoq = new Mock<ISejilSettings>();
            var provider = new SejilSqlProvider(settingsMoq.Object);

            // Act
            var sql = provider.GetPagedLogEntriesSql(2, 100, null, new LogQueryFilter { LevelFilter = levelFilter, ExceptionsOnly = exceptionsOnly });

            // Assert
            Assert.Equal(expectedSql, GetInnerPredicate(sql));
        }

        [Fact]
        public void GetPagedLogEntriesSql_returns_correct_sql_for_events_with_specified_dateRangeFilter()
        {
            // Arrange
            var d1 = new DateTime(2017, 8, 1);
            var d2 = new DateTime(2017, 8, 10);
            var settingsMoq = new Mock<ISejilSettings>();
            var provider = new SejilSqlProvider(settingsMoq.Object);

            // Act
            var sql = provider.GetPagedLogEntriesSql(2, 100, null, new LogQueryFilter { DateRangeFilter = new List<DateTime> { d1, d2 } });

            // Assert
            Assert.Equal("timestamp >= '2017-08-01' and timestamp < '2017-08-10'", GetInnerPredicate_ts(sql));
        }

        public static IEnumerable<object[]> GetPagedLogEntriesSql_TestData()
        {
            // ... =|!=|like|not like ...
            yield return new object[]
            {
                "prob1 = 'value1'",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING SUM(name = 'prob1' AND value = 'value1') > 0)",
            };
            yield return new object[]
            {
                "prob1 != 'value1'",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING SUM(name = 'prob1' AND value = 'value1') = 0)",
            };
            yield return new object[]
            {
                "prob1 like '%value1'",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING SUM(name = 'prob1' AND value LIKE '%value1') > 0)",
            };
            yield return new object[]
            {
                "prob1 not like '%value1'",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING SUM(name = 'prob1' AND value LIKE '%value1') = 0)",
            };
            // ... and|or ...
            yield return new object[]
            {
                "prob1 = 'value1' and prob2 = 'value2'",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING " +
                "SUM(name = 'prob1' AND value = 'value1') > 0 " +
                "AND SUM(name = 'prob2' AND value = 'value2') > 0)",
            };
            yield return new object[]
            {
                "prob1 = 'value1' and prob2 != 'value2'",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING " +
                "SUM(name = 'prob1' AND value = 'value1') > 0 " +
                "AND SUM(name = 'prob2' AND value = 'value2') = 0)",
            };
            yield return new object[]
            {
                "prob1 not like '%value1' or prob2 like '%value2'",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING " +
                "SUM(name = 'prob1' AND value LIKE '%value1') = 0 " +
                "OR SUM(name = 'prob2' AND value LIKE '%value2') > 0)",
            };
            yield return new object[]
            {
                "prob1 not like '%value1' or prob2 not like '%value2'",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING " +
                "SUM(name = 'prob1' AND value LIKE '%value1') = 0 " +
                "OR SUM(name = 'prob2' AND value LIKE '%value2') = 0)",
            };
            // (...) and|or ...
            yield return new object[]
            {
                "(prob1 = 'value1' or prob1 = 'value2') and prob3 like '%value3'",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING " +
                "(SUM(name = 'prob1' AND value = 'value1') > 0 " +
                "OR SUM(name = 'prob1' AND value = 'value2') > 0) " +
                "AND SUM(name = 'prob3' AND value LIKE '%value3') > 0)",
            };
            yield return new object[]
            {
                "(prob1 = 'value1' and prob2 = 'value2') or prob3 not like '%value3'",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING " +
                "(SUM(name = 'prob1' AND value = 'value1') > 0 " +
                "AND SUM(name = 'prob2' AND value = 'value2') > 0) " +
                "OR SUM(name = 'prob3' AND value LIKE '%value3') = 0)",
            };
            // ... and|or (...)
            yield return new object[]
            {
                "prob1 = 'value1' or (prob1 = 'value2' and prob3 like '%value3')",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING " +
                "SUM(name = 'prob1' AND value = 'value1') > 0 " +
                "OR (SUM(name = 'prob1' AND value = 'value2') > 0 " +
                "AND SUM(name = 'prob3' AND value LIKE '%value3') > 0))",
            };
            yield return new object[]
            {
                "prob1 = 'value1' and (prob2 = 'value2' or prob3 not like '%value3')",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING " +
                "SUM(name = 'prob1' AND value = 'value1') > 0 " +
                "AND (SUM(name = 'prob2' AND value = 'value2') > 0 " +
                "OR SUM(name = 'prob3' AND value LIKE '%value3') = 0))",
            };
            // (...) and|or (...)
            yield return new object[]
            {
                "(prob1 = 'value1' or prob1 = 'value2') and (prob3 like '%value3' and prob4 != 5)",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING " +
                "(SUM(name = 'prob1' AND value = 'value1') > 0 " +
                "OR SUM(name = 'prob1' AND value = 'value2') > 0) " +
                "AND (SUM(name = 'prob3' AND value LIKE '%value3') > 0 " +
                "AND SUM(name = 'prob4' AND CAST(value AS NUMERIC) = 5) = 0))",
            };
            yield return new object[]
            {
                "(prob1 = 'value1' or prob1 = 'value2') or (prob3 like '%value3' and prob4 != 55.55)",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING " +
                "(SUM(name = 'prob1' AND value = 'value1') > 0 " +
                "OR SUM(name = 'prob1' AND value = 'value2') > 0) " +
                "OR (SUM(name = 'prob3' AND value LIKE '%value3') > 0 " +
                "AND SUM(name = 'prob4' AND CAST(value AS NUMERIC) = 55.55) = 0))",
            };
            // mixed
            yield return new object[]
            {
                "(p1 = 'v' and @message = 'v') or @level='dbg'",
                "(id IN (SELECT logId FROM log_property GROUP BY logId HAVING SUM(name = 'p1' AND value = 'v') > 0) AND message = 'v') OR level = 'dbg'",
            };
            yield return new object[]
            {
                "(@message = 'v' and p1 = 'v') or @level='dbg'",
                "(message = 'v' AND id IN (SELECT logId FROM log_property GROUP BY logId HAVING SUM(name = 'p1' AND value = 'v') > 0)) OR level = 'dbg'",
            };
            yield return new object[]
            {
                "(p1 = 'v')",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING (SUM(name = 'p1' AND value = 'v') > 0))",
            };
            yield return new object[]
            {
                "(@message = 'v')",
                "(message = 'v')",
            };
            // Non property columns
            yield return new object[]
            {
                "@message like '%search%'",
                "message LIKE '%search%'",
            };
            yield return new object[]
            {
                "prob1 = 'value1' or @message like '%search%' and prob2 != 'value2'",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING SUM(name = 'prob1' AND value = 'value1') > 0) " +
                "OR message LIKE '%search%' " +
                "AND id IN (SELECT logId FROM log_property GROUP BY logId HAVING SUM(name = 'prob2' AND value = 'value2') = 0)",
            };
            // General text
            yield return new object[]
            {
                "search",
                "(message LIKE '%search%' OR exception LIKE '%search%' OR " +
                "id in (SELECT logId FROM log_property WHERE value LIKE '%search%'))",
            };
            // General text
            yield return new object[]
            {
                "\"search=;()\"",
                "(message LIKE '%search=;()%' OR exception LIKE '%search=;()%' OR " +
                "id in (SELECT logId FROM log_property WHERE value LIKE '%search=;()%'))",
            };
            // Quote escaping for a search string
            yield return new object[]
            {
                "'searchstr'",
                "(message LIKE '%''searchstr''%' OR exception LIKE '%''searchstr''%' OR " +
                "id in (SELECT logId FROM log_property WHERE value LIKE '%''searchstr''%'))",
            };
            // Quote escaping for a parsed query
            yield return new object[]
            {
                @"@message = 'This is a ''quote'''",
                "message = 'This is a ''quote'''",
            };
            // Booleans
            yield return new object[]
            {
                "@loggedIn = true or @loggedIn = false",
                "loggedIn = 'True' OR loggedIn = 'False'",
            };
            // Numerics, >, <, >=, <=
            yield return new object[]
            {
                "ElapsedMilliseconds >= 100 and ElapsedMilliseconds < 500",
                "id IN (SELECT logId FROM log_property GROUP BY logId HAVING " +
                "SUM(name = 'ElapsedMilliseconds' AND CAST(value AS NUMERIC) >= 100) > 0 AND " +
                "SUM(name = 'ElapsedMilliseconds' AND CAST(value AS NUMERIC) < 500) > 0)",
            };
        }

        public static IEnumerable<object[]> GetPagedLogEntriesSql_returns_correct_sql_for_events_with_specified_dateFilter_TestData()
        {
            yield return new object[]
            {
                "5m",
                "timestamp >= datetime('now', '-5 Minute')"
            };
            yield return new object[]
            {
                "15m",
                "timestamp >= datetime('now', '-15 Minute')"
            };
            yield return new object[]
            {
                "1h",
                "timestamp >= datetime('now', '-1 Hour')"
            };
            yield return new object[]
            {
                "6h",
                "timestamp >= datetime('now', '-6 Hour')"
            };
            yield return new object[]
            {
                "12h",
                "timestamp >= datetime('now', '-12 Hour')"
            };
            yield return new object[]
            {
                "24h",
                "timestamp >= datetime('now', '-24 Hour')"
            };
            yield return new object[]
            {
                "2d",
                "timestamp >= datetime('now', '-2 Day')"
            };
            yield return new object[]
            {
                "5d",
                "timestamp >= datetime('now', '-5 Day')"
            };
        }

        public static IEnumerable<object[]> GetPagedLogEntriesSql_returns_correct_sql_for_events_with_specified_filter_TestData()
        {
            yield return new object[]
            {
                "info",
                false,
                "level = 'info'"
            };
            yield return new object[]
            {
                null,
                true,
                "exception is not null"
            };
            yield return new object[]
            {
                "info",
                true,
                "level = 'info' AND exception is not null"
            };
        }

        private static string GetInnerPredicate(string sql) => sql.Replace(
@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    
    WHERE (", "").Replace(
@")
    ORDER BY timestamp DESC
    LIMIT 100 OFFSET 100
) l
LEFT JOIN log_property p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name", "");

        private static string GetInnerPredicate_ts(string sql) => sql.Replace(
@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    WHERE (", "").Replace(
@")
    
    ORDER BY timestamp DESC
    LIMIT 100 OFFSET 100
) l
LEFT JOIN log_property p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name", "");
    }
}