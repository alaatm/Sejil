using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Sejil.Configuration.Internal;
using Sejil.Routing.Internal;
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