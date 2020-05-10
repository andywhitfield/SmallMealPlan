using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SmallMealPlan.Data;

namespace SmallMealPlan.Web
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);

            services
                .AddAuthentication(o =>
                {
                    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(o =>
                {
                    o.LoginPath = "/signin";
                    o.LogoutPath = "/signout";
                    o.Cookie.HttpOnly = true;
                    o.Cookie.MaxAge = TimeSpan.FromDays(150);
                    o.ExpireTimeSpan = TimeSpan.FromDays(150);
                    o.SlidingExpiration = true;
                })
                .AddOpenIdConnect(options =>
                {
                    options.ClientId = "smallmealplan";
                    options.ClientSecret = "a00b51b9-28f4-417e-8a7f-940e91a998ee";

                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
                    options.Authority = "https://smallauth.nosuchblogger.com/";
                    options.Scope.Add("roles");

                    options.SecurityTokenValidator = new JwtSecurityTokenHandler
                    {
                        InboundClaimTypeMap = new Dictionary<string, string>()
                    };

                    options.TokenValidationParameters.NameClaimType = "name";
                    options.TokenValidationParameters.RoleClaimType = "role";

                    options.AccessDeniedPath = "/";
                });

            services.AddLogging(logging =>
            {
                logging.AddConsole(opt => opt.TimestampFormat = "[HH:mm:ss] ");
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Trace);
            });

            services.Configure<CookiePolicyOptions>(o =>
            {
                o.CheckConsentNeeded = context => false;
                o.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<SqliteDataContext>((serviceProvider, options) => {
                var sqliteConnectionString = Configuration.GetConnectionString("SmallMealPlan");
                serviceProvider.GetRequiredService<ILogger<Startup>>().LogInformation($"Using connection string: {sqliteConnectionString}");
                options.UseSqlite(sqliteConnectionString);                
            });
            services
                .AddScoped<IUserAccountRepository, UserAccountRepository>()
                .AddScoped<IMealPlannerRepository, MealPlannerRepository>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0).AddSessionStateTempDataProvider();
            services.AddRazorPages();
            services.AddCors();
            services.AddDistributedMemoryCache();
            services.AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(5));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Home/Error");

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(options => options.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"));

            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            scope.ServiceProvider.GetRequiredService<SqliteDataContext>().Database.Migrate();
        }
    }
}
