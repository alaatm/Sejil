using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Moq;
using Sejil.Configuration.Internal;
using Sejil.Data.Internal;
using Sejil.Models.Internal;
using Xunit;
using Xunit.Abstractions;
using Dapper;
using System.Linq;

namespace Sejil.Test.Data
{
    [Collection("Repository Tests")]
    public class SejilRepositoryTests
    {
        [Fact]
        public async Task SaveQueryAsync_saves_query()
        {
            // Arrange
            var db = Guid.NewGuid().ToString();
            InitializeDatabse(db);
            var settingsMoq = new Mock<ISejilSettings>();
            settingsMoq.SetupGet(p => p.SqliteDbPath).Returns(db);
            var repository = new SejilRepository(new SejilSqlProvider(settingsMoq.Object), settingsMoq.Object);

            var logQuery = new LogQuery
            {
                Id = 1,
                Name = "Test",
                Query = "q"
            };

            // Act
            var result = await repository.SaveQueryAsync(logQuery);

            // Assert
            Assert.True(result);
            var savedQueries = await repository.GetSavedQueriesAsync();
            Assert.Equal(1, savedQueries.Count());
            Assert.Equal(1, savedQueries.First().Id);
            Assert.Equal("Test", savedQueries.First().Name);
            Assert.Equal("q", savedQueries.First().Query);
        }

        [Fact]
        public async Task GetSavedQueriesAsync_returns_saved_queries()
        {
            // Arrange
            var db = Guid.NewGuid().ToString();
            InitializeDatabse(db);
            var settingsMoq = new Mock<ISejilSettings>();
            settingsMoq.SetupGet(p => p.SqliteDbPath).Returns(db);
            var repository = new SejilRepository(new SejilSqlProvider(settingsMoq.Object), settingsMoq.Object);
            await repository.SaveQueryAsync(new LogQuery { Name = "Test1", Query = "q1" });
            await repository.SaveQueryAsync(new LogQuery { Name = "Test2", Query = "q2" });

            // Act
            var queries = await repository.GetSavedQueriesAsync();

            // Assert
            Assert.Equal(2, queries.Count());
            Assert.Equal(1, queries.First().Id);
            Assert.Equal("Test1", queries.First().Name);
            Assert.Equal("q1", queries.First().Query);
            Assert.Equal(2, queries.Skip(1).First().Id);
            Assert.Equal("Test2", queries.Skip(1).First().Name);
            Assert.Equal("q2", queries.Skip(1).First().Query);
        }

        [Fact]
        public async Task GetEventsPageAsync_no_props_returns_events_page()
        {
            // Arrange
            var db = Guid.NewGuid().ToString();
            var settingsMoq = new Mock<ISejilSettings>();
            settingsMoq.SetupGet(p => p.SqliteDbPath).Returns(db);
            settingsMoq.SetupGet(p => p.PageSize).Returns(3);
            InitializeDatabse(db);
            var repository = new SejilRepository(new SejilSqlProvider(settingsMoq.Object), settingsMoq.Object);

            using (var conn = new SqliteConnection($"DataSource={db}"))
            {
                conn.Open();
                for (var i = 0; i < 10; i++)
                {
                    var cmd = new SqliteCommand($@"INSERT INTO log (id, message, messageTemplate, level, timestamp) 
                        VALUES ('{Guid.NewGuid().ToString()}', '{i}', '{i}', 'info', datetime(CURRENT_TIMESTAMP,'+{i} Hour'))", conn);
                    cmd.ExecuteNonQuery();
                }
            }

            // Act
            var logs = await repository.GetEventsPageAsync(2, DateTime.MinValue, null);

            // Assert
            Assert.Equal(3, logs.Count());
            Assert.Equal("6", logs.ElementAt(0).Message);
            Assert.Empty(logs.ElementAt(0).Properties);
            Assert.Equal("5", logs.ElementAt(1).Message);
            Assert.Empty(logs.ElementAt(1).Properties);
            Assert.Equal("4", logs.ElementAt(2).Message);
            Assert.Empty(logs.ElementAt(2).Properties);
        }

        [Fact]
        public async Task GetEventsPageAsync_returns_events_page()
        {
            // Arrange
            var db = Guid.NewGuid().ToString();
            var settingsMoq = new Mock<ISejilSettings>();
            settingsMoq.SetupGet(p => p.SqliteDbPath).Returns(db);
            settingsMoq.SetupGet(p => p.PageSize).Returns(3);
            InitializeDatabse(db);
            var repository = new SejilRepository(new SejilSqlProvider(settingsMoq.Object), settingsMoq.Object);

            using (var conn = new SqliteConnection($"DataSource={db}"))
            {
                conn.Open();
                for (var i = 0; i < 10; i++)
                {
                    var id = Guid.NewGuid().ToString();
                    var cmd = new SqliteCommand($@"INSERT INTO log (id, message, messageTemplate, level, timestamp) 
                        VALUES ('{id}', '{i}', '{i}', 'info', datetime(CURRENT_TIMESTAMP,'+{i} Hour'))", conn);
                    cmd.ExecuteNonQuery();

                    for (var j = 0; j < 2; j++)
                    {
                        cmd.CommandText = $@"INSERT INTO log_property (logId, name, value) VALUES 
                            ('{id}', 'n', '{i}_{j}')";
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // Act
            var logs = await repository.GetEventsPageAsync(4, DateTime.MinValue, null);

            // Assert
            Assert.Equal(1, logs.Count());
            Assert.Equal("0", logs.ElementAt(0).Message);
            Assert.NotNull(logs.ElementAt(0).Properties);
            Assert.Equal(2, logs.ElementAt(0).Properties.Count);
            Assert.Equal("0_0", logs.ElementAt(0).Properties.ElementAt(0).Value);
            Assert.Equal("0_1", logs.ElementAt(0).Properties.ElementAt(1).Value);
        }

        private void InitializeDatabse(string path)
        {
            using (var conn = new SqliteConnection($"DataSource={path}"))
            {
                conn.Open();
                var sql = ResourceHelper.GetEmbeddedResource("Sejil.db.sql");
                var cmd = new SqliteCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}