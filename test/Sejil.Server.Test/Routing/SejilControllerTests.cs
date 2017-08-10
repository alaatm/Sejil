// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sejil.Configuration.Internal;
using Sejil.Data.Internal;
using Sejil.Models.Internal;
using Sejil.Routing.Internal;
using Xunit;

namespace Sejil.Test.Routing
{
    public class SejilControllerTests
    {
        [Fact]
        public async Task GetIndexAsync_writes_app_html_to_response_stream()
        {
            // Arrange
            var appHtml = "<html>...</html>";

            var settingsMoq = new Mock<ISejilSettings>();
            settingsMoq.SetupGet(p => p.SejilAppHtml).Returns(appHtml).Verifiable();
            var controller = new SejilController(Mock.Of<ISejilRepository>(), settingsMoq.Object);
            var (contextMoq, responseMoq, bodyMoq) = CreateContextMoq();

            // Act
            await controller.GetIndexAsync(contextMoq.Object);

            // Assert
            settingsMoq.VerifyAll();
            responseMoq.VerifySet(p => p.ContentType = "text/html");
            var data = Encoding.UTF8.GetBytes(appHtml);
            bodyMoq.Verify(p => p.WriteAsync(data, 0, data.Length, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetEventsAsync_retreives_paged_events_from_repository()
        {
            // Arrange
            var page = 1;
            var startingTimestamp = DateTime.Now;

            var repositoryMoq = new Mock<ISejilRepository>();
            var controller = new SejilController(repositoryMoq.Object, Mock.Of<ISejilSettings>());

            // Act
            await controller.GetEventsAsync(CreateContextMoq().contextMoq.Object, page, startingTimestamp, new LogQueryFilter());

            // Assert
            repositoryMoq.Verify(p => p.GetEventsPageAsync(page, startingTimestamp, It.IsAny<LogQueryFilter>()), Times.Once);
        }

        [Fact]
        public async Task GetEventsAsync_sets_page_to_1_when_zero_then_retreives_paged_events_from_repository()
        {
            // Arrange
            var page = 0;
            var startingTimestamp = DateTime.Now;

            var repositoryMoq = new Mock<ISejilRepository>();
            var controller = new SejilController(repositoryMoq.Object, Mock.Of<ISejilSettings>());

            // Act
            await controller.GetEventsAsync(CreateContextMoq().contextMoq.Object, page, startingTimestamp, new LogQueryFilter());

            // Assert
            repositoryMoq.Verify(p => p.GetEventsPageAsync(page + 1, startingTimestamp, It.IsAny<LogQueryFilter>()), Times.Once);
        }

        [Fact]
        public async Task GetEventsAsync_passes_query_filter_to_repository()
        {
            // Arrange
            var qf = new LogQueryFilter
            {
                QueryText = "p=v",
                DateFilter = "5m",
                DateRangeFilter = new List<DateTime>() { DateTime.Now, DateTime.Now }
            };
            var repositoryMoq = new Mock<ISejilRepository>();
            var controller = new SejilController(repositoryMoq.Object, Mock.Of<ISejilSettings>());

            // Act
            await controller.GetEventsAsync(CreateContextMoq().contextMoq.Object, 1, null, qf);

            // Assert
            repositoryMoq.Verify(p => p.GetEventsPageAsync(1, null, It.Is<LogQueryFilter>(f => f == qf)), Times.Once);
        }

        [Fact]
        public async Task GetEventsAsync_writes_events_json_to_response_stream()
        {
            // Arrange
            var (logEntries, logEntriesJson) = GetTestLogEntries();
            var repositoryMoq = new Mock<ISejilRepository>();
            repositoryMoq.Setup(p => p.GetEventsPageAsync(1, null, null)).ReturnsAsync(logEntries);
            var controller = new SejilController(repositoryMoq.Object, Mock.Of<ISejilSettings>());
            var (contextMoq, responseMoq, bodyMoq) = CreateContextMoq();

            // Act
            await controller.GetEventsAsync(contextMoq.Object, 1, null, null);

            // Assert
            responseMoq.VerifySet(p => p.ContentType = "application/json");
            var data = Encoding.UTF8.GetBytes(logEntriesJson);
            bodyMoq.Verify(p => p.WriteAsync(data, 0, data.Length, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SaveQueryAsync_calls_repository_SaveQueryAsync()
        {
            // Arrange
            var logQuery = new LogQuery
            {
                Name = "test",
                Query = "query"
            };
            var repositoryMoq = new Mock<ISejilRepository>();
            var controller = new SejilController(repositoryMoq.Object, Mock.Of<ISejilSettings>());

            // Act
            await controller.SaveQueryAsync(CreateContextMoq().contextMoq.Object, logQuery);

            // Assert
            repositoryMoq.Verify(p => p.SaveQueryAsync(logQuery), Times.Once);
        }

        [Theory]
        [InlineData(true, StatusCodes.Status201Created)]
        [InlineData(false, StatusCodes.Status500InternalServerError)]
        public async Task SaveQueryAsync_sets_status_code_based_on_save_operation_success(bool saveOperationResult, int expectedStatusCode)
        {
            // Arrange
            var repositoryMoq = new Mock<ISejilRepository>();
            repositoryMoq.Setup(p => p.SaveQueryAsync(It.IsAny<LogQuery>())).ReturnsAsync(saveOperationResult);
            var controller = new SejilController(repositoryMoq.Object, Mock.Of<ISejilSettings>());
            var (contextMoq, responseMoq, _) = CreateContextMoq();

            // Act
            await controller.SaveQueryAsync(contextMoq.Object, new LogQuery());

            // Assert
            responseMoq.VerifySet(p => p.StatusCode = expectedStatusCode);
        }

        [Fact]
        public async Task GetQueriesAsync_retreives_saved_queries_from_repository()
        {
            // Arrange
            var repositoryMoq = new Mock<ISejilRepository>();
            var controller = new SejilController(repositoryMoq.Object, Mock.Of<ISejilSettings>());

            // Act
            await controller.GetQueriesAsync(CreateContextMoq().contextMoq.Object);

            // Assert
            repositoryMoq.Verify(p => p.GetSavedQueriesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetQueriesAsync_writes_queries_json_to_response_stream()
        {
            // Arrange
            var (logQueries, logQueriesJson) = GetTestLogQueries();
            var repositoryMoq = new Mock<ISejilRepository>();
            repositoryMoq.Setup(p => p.GetSavedQueriesAsync()).ReturnsAsync(logQueries);
            var controller = new SejilController(repositoryMoq.Object, Mock.Of<ISejilSettings>());
            var (contextMoq, responseMoq, bodyMoq) = CreateContextMoq();

            // Act
            await controller.GetQueriesAsync(contextMoq.Object);

            // Assert
            responseMoq.VerifySet(p => p.ContentType = "application/json");
            var data = Encoding.UTF8.GetBytes(logQueriesJson);
            bodyMoq.Verify(p => p.WriteAsync(data, 0, data.Length, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void SetMinimumLogLevel_calls_settings_TrySetMinimumLogLevel()
        {
            // Arrange
            var logLevel = "info";
            var settingsMoq = new Mock<ISejilSettings>();
            var controller = new SejilController(Mock.Of<ISejilRepository>(), settingsMoq.Object);

            // Act
            controller.SetMinimumLogLevel(CreateContextMoq().contextMoq.Object, logLevel);

            // Assert
            settingsMoq.Verify(p => p.TrySetMinimumLogLevel(logLevel), Times.Once);
        }

        [Theory]
        [InlineData(true, StatusCodes.Status200OK)]
        [InlineData(false, StatusCodes.Status400BadRequest)]
        public void SetMinimumLogLevel_sets_status_code_based_on_save_operation_success(bool saveOperationResult, int expectedStatusCode)
        {
            // Arrange
            var settingsMoq = new Mock<ISejilSettings>();
            settingsMoq.Setup(p => p.TrySetMinimumLogLevel(It.IsAny<string>())).Returns(saveOperationResult);
            var controller = new SejilController(Mock.Of<ISejilRepository>(), settingsMoq.Object);
            var (contextMoq, responseMoq, _) = CreateContextMoq();

            // Act
            controller.SetMinimumLogLevel(contextMoq.Object, "info");

            // Assert
            responseMoq.VerifySet(p => p.StatusCode = expectedStatusCode);
        }

        private (IEnumerable<LogEntry> logEntries, string logEntriesJson) GetTestLogEntries()
        {
            var events = new List<LogEntry>()
            {
                new LogEntry
                {
                    Id = "001",
                    Message = "message",
                    MessageTemplate = "message template",
                    Properties = new List<LogEntryProperty>
                    {
                        new LogEntryProperty
                        {
                            Id = 1,
                            Name = "prop1",
                            Value = "value1"
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(events,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            return (events, json);
        }

        private (IEnumerable<LogQuery> logQueries, string logQueriesJson) GetTestLogQueries()
        {
            var queries = new List<LogQuery>
            {
                new LogQuery
                {
                    Name = "q1",
                    Query = "query1"
                },
                new LogQuery
                {
                    Name = "q2",
                    Query = "query2"
                },
            };

            var json = JsonConvert.SerializeObject(queries,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            return (queries, json);
        }

        private (Mock<HttpContext> contextMoq, Mock<HttpResponse> responseMoq, Mock<Stream> bodyMoq) CreateContextMoq()
        {
            var bodyMoq = new Mock<Stream>();
            var responseMoq = new Mock<HttpResponse>();
            responseMoq.SetupGet(p => p.Body).Returns(bodyMoq.Object);
            var contextMoq = new Mock<HttpContext>();
            contextMoq.Setup(p => p.Response).Returns(responseMoq.Object);

            return (contextMoq, responseMoq, bodyMoq);
        }
    }
}