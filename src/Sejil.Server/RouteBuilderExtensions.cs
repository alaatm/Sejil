using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Sejil
{
    internal static class RouteBuilderExtensions
    {
        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP GET requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapGet(this IRouteBuilder builder, string template, RequestDelegate handler)
        {
            return builder.MapVerb("GET", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP POST requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapPost(this IRouteBuilder builder, string template, RequestDelegate handler)
        {
            return builder.MapVerb("POST", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP requests for the given
        /// <paramref name="verb"/>, <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="verb">The HTTP verb allowed by the route.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The route handler.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapVerb(
            this IRouteBuilder builder,
            string verb,
            string template,
            Func<HttpRequest, HttpResponse, RouteData, Task> handler)
        {
            RequestDelegate requestDelegate = (httpContext) =>
            {
                return handler(httpContext.Request, httpContext.Response, httpContext.GetRouteData());
            };

            return builder.MapVerb(verb, template, requestDelegate);
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP requests for the given
        /// <paramref name="verb"/>, <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="verb">The HTTP verb allowed by the route.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapVerb(
            this IRouteBuilder builder,
            string verb,
            string template,
            RequestDelegate handler)
        {
            var route = new Route(
                new RouteHandler(handler),
                template,
                defaults: null,
                constraints: new RouteValueDictionary(new { httpMethod = new HttpMethodRouteConstraint(verb) }),
                dataTokens: null,
                inlineConstraintResolver: GetConstraintResolver(builder));

            builder.Routes.Add(route);
            return builder;
        }

        private static IInlineConstraintResolver GetConstraintResolver(IRouteBuilder builder)
        {
            return builder.ServiceProvider.GetRequiredService<IInlineConstraintResolver>();
        }

        /// <summary>
        /// Adds a <see cref="RouterMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>
        /// with the <see cref="IRouter"/> built from configured <see cref="IRouteBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="action">An <see cref="Action{IRouteBuilder}"/> to configure the provided <see cref="IRouteBuilder"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseRouter(this IApplicationBuilder builder, Action<IRouteBuilder> action)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (builder.ApplicationServices.GetService(typeof(RoutingMarkerService)) == null)
            {
                throw new InvalidOperationException($"Unable to find service of type {nameof(RoutingMarkerService)}");
            }

            var routeBuilder = new RouteBuilder(builder);
            action(routeBuilder);

            return builder.UseRouter(routeBuilder.Build());
        }
    }
}