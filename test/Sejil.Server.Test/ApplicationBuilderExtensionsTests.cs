// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sejil.Configuration.Internal;
using Sejil.Models.Internal;
using Sejil.Routing.Internal;
using Serilog.Events;
using Xunit;
using Microsoft.AspNetCore.Builder;
using Sejil.Data.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Security.Claims;
using System.Net;
using System.Text.Json;
using System.IO;

namespace Sejil.Test
{
    public class ApplicationBuilderExtensionsTests
    {
        internal static readonly JsonSerializerOptions _camelCaseJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        [Fact]
        public async Task HttpGet_root_url_calls_controller_GetIndexAsync_method()
        {
            // Arrange
            var url = "/sejil";
            var target = url;
            var controllerMoq = new Mock<ISejilController>();
            var server = CreateServer(url, controllerMoq.Object);

            // Act
            await server.CreateClient().GetAsync(target);

            // Assert
            controllerMoq.Verify(p => p.GetIndexAsync(), Times.Once);
        }

        [Theory]
        [MemberData(nameof(HttpPost_events_url_calls_controller_GetEventsAsync_method_TestData))]
        public async Task HttpPost_events_url_calls_controller_GetEventsAsync_method(string queryString,
            string bodyContent, int expectedPageArg, DateTime? expectedstartingTsArg, LogQueryFilter expectedQueryFilterArg)
        {
            // Arrange
            var url = "/sejil";
            var target = $"{url}/events{queryString}";
            var controllerMoq = new Mock<ISejilController>();
            var server = CreateServer(url, controllerMoq.Object);
            var content = bodyContent == null
                ? null
                : new StringContent(bodyContent);

            // Act
            await server.CreateClient().PostAsync(target, content);

            // Assert
            controllerMoq.Verify(p => p.GetEventsAsync(
                expectedPageArg, expectedstartingTsArg, It.Is<LogQueryFilter>(qf =>
                qf.QueryText == expectedQueryFilterArg.QueryText &&
                qf.DateFilter == expectedQueryFilterArg.DateFilter &&
                Join(qf.DateRangeFilter) == Join(expectedQueryFilterArg.DateRangeFilter))), Times.Once);
        }

        [Fact]
        public async Task HttpPost_log_query_url_calls_controller_SaveQueryAsync_method()
        {
            // Arrange
            var url = "/sejil";
            var target = $"{url}/log-query";
            var controllerMoq = new Mock<ISejilController>();
            var server = CreateServer(url, controllerMoq.Object);
            var logQuery = new LogQuery
            {
                Name = "Test",
                Query = "Test"
            };
            var content = new StringContent(JsonSerializer.Serialize(logQuery, _camelCaseJson));

            // Act
            await server.CreateClient().PostAsync(target, content);

            // Assert
            controllerMoq.Verify(p => p.SaveQueryAsync(
                It.Is<LogQuery>(v => v.Name == logQuery.Name && v.Query == logQuery.Query)), Times.Once);
        }

        [Fact]
        public async Task HttpGet_log_queries_url_calls_controller_GetQueriesAsync_method()
        {
            // Arrange
            var url = "/sejil";
            var target = $"{url}/log-queries";
            var controllerMoq = new Mock<ISejilController>();
            var server = CreateServer(url, controllerMoq.Object);

            // Act
            await server.CreateClient().GetAsync(target);

            // Assert
            controllerMoq.Verify(p => p.GetQueriesAsync(), Times.Once);
        }

        [Fact]
        public async Task HttpGet_min_log_level_url_calls_controller_GetMinimumLogLevelAsync_method()
        {
            // Arrange
            var url = "/sejil";
            var target = $"{url}/min-log-level";
            var controllerMoq = new Mock<ISejilController>();
            var server = CreateServer(url, controllerMoq.Object);

            // Act
            await server.CreateClient().GetAsync(target);

            // Assert
            controllerMoq.Verify(p => p.GetMinimumLogLevelAsync(), Times.Once);
        }

        [Fact]
        public async Task HttpPost_min_log_level_url_calls_controller_SetMinimumLogLevel_method()
        {
            // Arrange
            var url = "/sejil";
            var target = $"{url}/min-log-level";
            var targetMinLogLevel = "info";
            var controllerMoq = new Mock<ISejilController>();
            var server = CreateServer(url, controllerMoq.Object);
            var content = new StringContent(targetMinLogLevel);

            // Act
            await server.CreateClient().PostAsync(target, content);

            // Assert
            controllerMoq.Verify(p => p.SetMinimumLogLevel(targetMinLogLevel), Times.Once);
        }

        [Fact]
        public async Task HttpPost_del_query_url_calls_controller_DeleteQueryAsync_method()
        {
            // Arrange
            var url = "/sejil";
            var query = "query";
            var target = $"{url}/del-query";
            var controllerMoq = new Mock<ISejilController>();
            var server = CreateServer(url, controllerMoq.Object);
            var content = new StringContent(query);

            // Act
            await server.CreateClient().PostAsync(target, content);

            // Assert
            controllerMoq.Verify(p => p.DeleteQueryAsync(query), Times.Once);
        }

        [Fact]
        public async Task HttpGet_title_calls_controller_GetTitleAsync_method()
        {
            // Arrange
            var url = "/sejil";
            var target = $"{url}/title";
            var controllerMoq = new Mock<ISejilController>();
            var server = CreateServer(url, controllerMoq.Object);

            // Act
            await server.CreateClient().GetAsync(target);

            // Assert
            controllerMoq.Verify(p => p.GetTitleAsync(), Times.Once);
        }

        [Fact]
        public async Task HttpGet_root_url_succeeds_when_auth_is_required_and_user_is_authenticated()
        {
            // Arrange
            var url = "/sejil";
            var server = CreateServerWithAuth(url, true, "username");

            // Act
            var response = await server.CreateClient().GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HttpGet_root_url_fails_when_auth_is_required_and_user_is_not_authenticated()
        {
            // Arrange
            var url = "/sejil";
            var server = CreateServerWithAuth(url, false);

            // Act
            var response = await server.CreateClient().GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task HttpGet_username_returns_authenticated_username_when_authenticated()
        {
            // Arrange
            var url = "/sejil";
            var username = "username";
            var server = CreateServerWithAuth(url, true, username);

            // Act
            var response = await server.CreateClient().GetAsync("/sejil/user-name");

            // Assert
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("{\"userName\":\"" + username + "\"}", result);
        }

        [Fact]
        public async Task HttpGet_username_returns_emptyString_when_not_authenticated()
        {
            // Arrange
            var url = "/sejil";
            var server = CreateServerWithAuth(url, false);

            // Act
            var response = await server.CreateClient().GetAsync("/sejil/user-name");

            // Assert
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("{\"userName\":\"\"}", result);
        }

        [Fact]
        public async Task HttpPost_with_empty_content_doesnt_crash()
        {
            // Arrange
            var controllerMoq = new Mock<ISejilController>();
            var server = CreateServer("/sejil", controllerMoq.Object);
            var content = new StringContent("");

            // Act
            await server.CreateClient().PostAsync("/sejil/del-query", content);

            // Assert
            controllerMoq.Verify(p => p.DeleteQueryAsync(null), Times.Once);
        }

        public static IEnumerable<object[]> HttpPost_events_url_calls_controller_GetEventsAsync_method_TestData()
        {
            yield return new object[]
            {
                "", "{\"queryText\":\"\",\"dateFilter\":null,\"dateRangeFilter\":null}", 0, null, new LogQueryFilter { QueryText = "" }
            };
            yield return new object[]
            {
                "", "{\"queryText\":\"my query\",\"dateFilter\":\"5m\",\"dateRangeFilter\":null}", 0, null, new LogQueryFilter { QueryText = "my query", DateFilter = "5m"}
            };
            yield return new object[]
            {
                "", "{\"queryText\":\"my query\",\"dateFilter\":null,\"dateRangeFilter\":[\"2017-08-01\",\"2017-08-10\"]}", 0, null,
                    new LogQueryFilter { QueryText = "my query", DateFilter = null, DateRangeFilter = new List<DateTime> { new DateTime(2017, 8, 1), new DateTime(2017, 8, 10) }}
            };
            yield return new object[]
            {
                "?page=2", "{\"queryText\":\"my query\",\"dateFilter\":null,\"dateRangeFilter\":null}", 2, null, new LogQueryFilter { QueryText = "my query" }
            };
            yield return new object[]
            {
                "?page=2&startingTs=2017-08-04%2006%3A07%3A44.100", "{\"queryText\":\"my query\",\"dateFilter\":null,\"dateRangeFilter\":null}", 2, new DateTime(2017, 8, 4, 6, 7, 44, 100), new LogQueryFilter { QueryText = "my query" }
            };
        }

        private static TestServer CreateServer(string url, ISejilController controller)
        {
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    // Workaround for https://github.com/dotnet/aspnetcore/issues/18463
                    // so that ContentLength gets properly set for the test server.
                    app.Use(async (context, next) =>
                    {
                        context.Request.EnableBuffering();
                        using var ms = new MemoryStream();
                        await context.Request.Body.CopyToAsync(ms);
                        context.Request.ContentLength = ms.Length;
                        context.Request.Body.Seek(0, SeekOrigin.Begin);
                        await next.Invoke();
                    });

                    app.UseSejil();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ISejilSettings>(new SejilSettings(url, LogEventLevel.Debug));
                    services.AddSingleton(controller);
                    services.AddRouting();
                });

            return new TestServer(builder);
        }

        private static TestServer CreateServerWithAuth(string url, bool authenticated, string username = null)
        {
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseAuthentication();
                    app.UseSejil();
                })
                .ConfigureServices(services =>
                {
                    if (authenticated)
                    {
                        services.AddAuthentication(TestAuthDefaults.AuthenticationScheme).AddAlwaysAuthenticated(username);
                    }
                    else
                    {
                        services.AddAuthentication(TestAuthDefaults.AuthenticationScheme).AddAlwaysNotAuthenticated();
                    }
                    services.AddRouting();

                    services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    services.AddSingleton<ISejilSettings>(new SejilSettings(url, LogEventLevel.Debug));
                    services.AddScoped<ISejilRepository, SejilRepository>();
                    services.AddScoped<ISejilSqlProvider, SejilSqlProvider>();
                    services.AddScoped<ISejilController, SejilController>();

                    services.ConfigureSejil(options => options.AuthenticationScheme = TestAuthDefaults.AuthenticationScheme);
                });

            return new TestServer(builder);
        }

        static string Join(List<DateTime> dateList)
        {
            if (dateList == null || dateList?.Count == 0)
            {
                return null;
            }

            return string.Join(",", dateList);
        }
    }

    public static class TestAuthDefaults
    {
        public static string AuthenticationScheme { get; } = "TestAuthScheme";
    }

    public class TestAuthOptions : AuthenticationSchemeOptions
    {
        public string Username { get; set; }
    }

    public class TestAuthHandler : AuthenticationHandler<TestAuthOptions>
    {
        public TestAuthHandler(IOptionsMonitor<TestAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!string.IsNullOrWhiteSpace(Options.Username))
            {
                var claims = new[] { new Claim(ClaimTypes.Name, Options.Username, ClaimValueTypes.String, ClaimsIssuer) };
                var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
                var authTicket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(authTicket));
            }
            else
            {
                return Task.FromResult(AuthenticateResult.Fail("fail"));
            }
        }
    }

    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddAlwaysAuthenticated(this AuthenticationBuilder builder, string username)
            => builder.AddScheme<TestAuthOptions, TestAuthHandler>(TestAuthDefaults.AuthenticationScheme, options => options.Username = username);

        public static AuthenticationBuilder AddAlwaysNotAuthenticated(this AuthenticationBuilder builder)
            => builder.AddScheme<TestAuthOptions, TestAuthHandler>(TestAuthDefaults.AuthenticationScheme, _ => { });
    }
}