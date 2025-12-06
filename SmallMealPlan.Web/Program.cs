using SmallMealPlan.Web;

var host = new HostBuilder()
    .ConfigureWebHost(webHostBuilder =>
    {
        webHostBuilder
#if DEBUG
            .UseKestrel()
#else
            .UseIIS()
#endif
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<Startup>();
    }).Build();

host.Run();
