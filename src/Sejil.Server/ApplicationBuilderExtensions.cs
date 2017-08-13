// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Sejil.Configuration.Internal;
using Sejil.Routing.Internal;
using Newtonsoft.Json;
using Sejil.Models.Internal;
#if NETSTANDARD1_6
using Sejil.Routing;
#elif NETSTANDARD2_0
using Microsoft.AspNetCore.Routing;
#endif

namespace Sejil
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds Sejil to the request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseSejil(this IApplicationBuilder app)
        {
            var settings = app.ApplicationServices.GetService(typeof(ISejilSettings)) as SejilSettings;
            var url = settings.Url.Substring(1); // Skip the '/'

            app.UseRouter(routes =>
            {
                routes.MapGet(url, async context =>
                {
                    var controller = GetSejilController(context);
                    await controller.GetIndexAsync(context);
                });

                routes.MapPost($"{url}/events", async context =>
                {
                    var query = JsonConvert.DeserializeObject<LogQueryFilter>(
                        await GetRequestBodyAsync(context.Request));
                    Int32.TryParse(context.Request.Query["page"].FirstOrDefault(), out var page);
                    var dateParsed = DateTime.TryParse(context.Request.Query["startingTs"].FirstOrDefault(), out var startingTs);

                    var controller = GetSejilController(context);
                    await controller.GetEventsAsync(context, page, dateParsed ? startingTs : (DateTime?)null, query);
                });

                routes.MapPost($"{url}/log-query", async context =>
                {
                    var logQuery = JsonConvert.DeserializeObject<LogQuery>(
                        await GetRequestBodyAsync(context.Request));

                    var controller = GetSejilController(context);
                    await controller.SaveQueryAsync(context, logQuery);
                });

                routes.MapGet($"{url}/log-queries", async context =>
                {
                    var controller = GetSejilController(context);
                    await controller.GetQueriesAsync(context);
                });

                routes.MapPost($"{url}/min-log-level", async context =>
                {
                    var minLogLevel = await GetRequestBodyAsync(context.Request);
                    var controller = GetSejilController(context);
                    controller.SetMinimumLogLevel(context, minLogLevel);
                });

                routes.MapPost($"{url}/del-query", async context =>
                {
                    var queryName = await GetRequestBodyAsync(context.Request);
                    var controller = GetSejilController(context);
                    await controller.DeleteQueryAsync(context, queryName);
                });
            });

            return app;
        }

        private static ISejilController GetSejilController(HttpContext context)
            => context.RequestServices.GetService(typeof(ISejilController)) as ISejilController;

        private static async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            // TODO: Remove below try-catch and use request.ContentLength directly
            // once test issue is fixed.
            long length = 0;
            try
            {
                length = request.Body?.Length ?? 0;
            }
            catch
            {
                length = request.ContentLength ?? 0;
            }

            if (length > 0)
            {
                var buffer = new byte[(int)length];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer);
            }

            return null;
        }
    }
}