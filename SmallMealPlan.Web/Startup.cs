﻿using System;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.RememberTheMilk;
using SmallMealPlan.SmallLister;
using SmallMealPlan.Web.Authorisation;

namespace SmallMealPlan.Web;

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
        Environment = env;
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment Environment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(Configuration);

        services
            .ConfigureApplicationCookie(c => c.Cookie.Name = "smallmealplan")
            .AddAuthentication(o => o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(o =>
            {
                o.LoginPath = "/signin";
                o.LogoutPath = "/signout";
                o.Cookie.Name = "smallmealplan";
                o.Cookie.HttpOnly = true;
                o.Cookie.MaxAge = TimeSpan.FromDays(7);
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.Cookie.IsEssential = true;
                o.ExpireTimeSpan = TimeSpan.FromDays(7);
                o.SlidingExpiration = true;
            });

        services
            .AddDataProtection()
            .SetApplicationName(typeof(Startup).Namespace ?? "")
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Environment.ContentRootPath, ".keys")));

        services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
            logging.SetMinimumLevel(LogLevel.Trace);
        });

        services.Configure<SmallMealPlanConfig>(Configuration);
        services.Configure<CookiePolicyOptions>(o =>
        {
            o.CheckConsentNeeded = context => false;
            o.MinimumSameSitePolicy = SameSiteMode.None;
        });

        services.AddDbContext<SqliteDataContext>((serviceProvider, options) =>
        {
            var sqliteConnectionString = Configuration.GetConnectionString("SmallMealPlan");
            serviceProvider.GetRequiredService<ILogger<Startup>>().LogInformation($"Using connection string: {sqliteConnectionString}");
            options.UseSqlite(sqliteConnectionString);
        });
        services.AddHttpClient(RtmClient.HttpClientName);
        services
            .AddScoped<IUserAccountRepository, UserAccountRepository>()
            .AddScoped<IPlannerMealRepository, PlannerMealRepository>()
            .AddScoped<IMealRepository, MealRepository>()
            .AddScoped<IShoppingListRepository, ShoppingListRepository>()
            .AddScoped<INoteRepository, NoteRepository>()
            .AddScoped<IDirectDbService, DirectDbService>()
            .AddScoped<IAuthorisationHandler, AuthorisationHandler>()
            .AddSingleton(new RtmConfig(Configuration.GetValue<string>("RememberTheMilk:ApiKey") ?? "", Configuration.GetValue<string>("RememberTheMilk:SharedSecret") ?? ""))
            .AddScoped<IRtmClient, RtmClient>()
            .AddSingleton(new SmallListerConfig(Configuration.GetValue<Uri>("SmallLister:BaseUri") ?? throw new InvalidOperationException("Missing SmallLister:BaseUri config"), Configuration.GetValue<string>("SmallLister:AppKey") ?? "", Configuration.GetValue<string>("SmallLister:AppSecret") ?? ""))
            .AddScoped<ISmallListerClient, SmallListerClient>()
            .AddSingleton<ISmallListerSendQueue, SmallListerSendQueue>()
            .AddHostedService<SmallListerSendQueueHostedService>();

        services.AddMvc().AddSessionStateTempDataProvider();
        services.AddRazorPages();
        services.AddCors();
        services.AddDistributedMemoryCache();
        services
            .AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(5))
            .AddFido2(options =>
            {
                options.ServerName = "Small:MealPlan";
                options.ServerDomain = Configuration.GetValue<string>("FidoDomain");
                options.Origins = [Configuration.GetValue<string>("FidoOrigins")];
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
