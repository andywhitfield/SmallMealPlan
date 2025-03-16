using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SmallMealPlan.Data;
using SmallMealPlan.Model;

namespace SmallMealPlan.Web.Tests;

public class WebApplicationFactoryTest : WebApplicationFactory<Startup>
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SqliteDataContext> _options;
    private UserAccount? _testUser;

    public WebApplicationFactoryTest()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<SqliteDataContext>().UseSqlite(_connection).Options;
    }

    public UserAccount TestUser => _testUser ?? throw new InvalidOperationException("Test user not created");

    protected override IHostBuilder CreateHostBuilder()
        => Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(x => x.UseStartup<Startup>().UseTestServer().ConfigureTestServices(services =>
        {
            services.Replace(ServiceDescriptor.Scoped(_ => new SqliteDataContext(_options)));
            services
                .AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestStubAuthHandler>("Test", null);
        }));

    public async Task<HttpClient> CreateUnauthenticatedClientAsync(bool allowAutoRedirect = false)
    {
        await CreateTestUserAsync();
        return CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = allowAutoRedirect });
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(bool allowAutoRedirect = false)
    {
        await CreateTestUserAsync();
        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = allowAutoRedirect });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        return client;
    }

    public async Task CreateTestUserAsync()
    {
        if (_testUser != null)
            return;

        await using var serviceScope = Services.CreateAsyncScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        _testUser = context.UserAccounts.Add(new() { Email = "test-user-1" }).Entity;
        await context.SaveChangesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _connection.Dispose();
    }

    public static string GetFormValidationToken(string responseContent, string formAction)
    {
        var formStart = responseContent.IndexOf($"<form method=\"post\" action=\"{formAction}\"");
        if (formStart < 0)
            throw new InvalidOperationException($"Cannot find form with action [{formAction}] in response: {responseContent}");
        var validationToken = responseContent.Substring(formStart);
        validationToken = validationToken.Substring(validationToken.IndexOf("__RequestVerificationToken"));
        validationToken = validationToken.Substring(validationToken.IndexOf("value=\"") + 7);
        validationToken = validationToken.Substring(0, validationToken.IndexOf('"'));
        return validationToken;
    }
}
