using Microsoft.Extensions.DependencyInjection;
using Sejil.Configuration.Internal;
using Serilog.Events;

namespace Sejil.Test
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void ConfigureSejil_throws_when_setupAction_is_null()
        {
            // Arrange, act & assert
            var ex = Assert.Throws<ArgumentNullException>(() => new ServiceCollection().ConfigureSejil(null));
            Assert.Equal("setupAction", ex.ParamName);
        }

        [Fact]
        public void ConfigureSejil_executes_setupAction()
        {
            // Arrange
            var authScheme = "authScheme";
            var title = "title";
            var settings = new SejilSettings("/sejil", LogEventLevel.Debug);
            var services = new ServiceCollection().AddSingleton<ISejilSettings>(settings);

            // Act
            services.ConfigureSejil(options =>
            {
                options.AuthenticationScheme = authScheme;
                options.Title = title;
            });

            // Assert
            Assert.Equal(authScheme, settings.AuthenticationScheme);
            Assert.Equal(title, settings.Title);
        }
    }
}
