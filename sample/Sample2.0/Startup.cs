// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sejil;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using sample.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            //// Basic auth
            //services.AddAuthentication()
            //   .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                Configuration.GetSection("Authentication").Bind(options);
              
                options.ClaimActions.MapUniqueJsonKey("sub", "sub");
                options.ClaimActions.MapUniqueJsonKey("name", "name");
                options.ClaimActions.MapUniqueJsonKey("given_name", "given_name");
                options.ClaimActions.MapUniqueJsonKey("family_name", "family_name");
                options.ClaimActions.MapUniqueJsonKey("profile", "profile");
                options.ClaimActions.MapUniqueJsonKey("email", "email");

                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };

            });

            // configure DI for application services
            services.AddScoped<IUserService, UserService>();

            services.ConfigureSejil(options =>
            {
                options.AuthenticationScheme = OpenIdConnectDefaults.AuthenticationScheme;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseSejil();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}