using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Serilog;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using Sejil.Logging.Sinks;

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
            var settings = app.ApplicationServices.GetService(typeof(SejilSettings)) as SejilSettings;
            var url = settings.Uri.Substring(1); // Skip the '/'

            app.UseRouter(routes =>
            {
                routes.MapGet(url, async context =>
                {
                    var controller = GetSejilController(context);
                    await controller.Index(context);
                });

                routes.MapPost($"{url}/events", async context =>
                {
                    var controller = GetSejilController(context);
                    await controller.GetEvents(context);
                });

                routes.MapPost($"{url}/log-query", async context =>
                {
                    var controller = GetSejilController(context);
                    await controller.SaveQuery(context);
                });

                routes.MapGet($"{url}/log-queries", async context =>
                {
                    var controller = GetSejilController(context);
                    await controller.GetQueries(context);
                });

                routes.MapPost($"{url}/min-log-level", async context =>
                {
                    var controller = GetSejilController(context);
                    await controller.SetMinimumLogLevel(context);
                });
            });

            return app;
        }

        private static ISejilController GetSejilController(HttpContext context)
            => context.RequestServices.GetService(typeof(ISejilController)) as ISejilController;
    }
}