using Sejil.Configuration.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension to conf
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure Sejil
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="setupAction">Delegate to configure the settings.</param>		
        public static void ConfigureSejil(this IServiceCollection services, Action<ISejilSettings> setupAction)
        {
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            var settings = services.BuildServiceProvider().GetService<ISejilSettings>();

            setupAction(settings);
        }
    }
}
