using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallMealPlan.Data;

namespace SmallMealPlan.Web.Tests;

[TestClass]
public class CurrentAreaTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();

    [TestMethod]
    public async Task Given_no_current_area_should_default_to_planner()
    {
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var homeResponse = await client.GetAsync("/");
        Assert.AreEqual(HttpStatusCode.Redirect, homeResponse.StatusCode);
        Assert.AreEqual(new Uri("/planner", UriKind.Relative), homeResponse.Headers.Location);

        await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
        var db = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var user = await db.UserAccounts.SingleAsync(ua => ua.UserAccountId == _webApplicationFactory.TestUser.UserAccountId);
        Assert.AreEqual("/planner", user.CurrentArea);
    }

    [TestMethod]
    [DataRow("/planner")]
    [DataRow("/meals")]
    [DataRow("/shoppinglist")]
    [DataRow("/notes")]
    public async Task Given_current_area_should_redirect(string currentArea)
    {
        {
            await _webApplicationFactory.CreateTestUserAsync();
            await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
            var db = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
            var user = await db.UserAccounts.SingleAsync(ua => ua.UserAccountId == _webApplicationFactory.TestUser.UserAccountId);
            user.CurrentArea = currentArea;
            await db.SaveChangesAsync();
        }

        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var homeResponse = await client.GetAsync("/");
        Assert.AreEqual(HttpStatusCode.Redirect, homeResponse.StatusCode);
        Assert.AreEqual(new Uri(currentArea, UriKind.Relative), homeResponse.Headers.Location);
    }

    [TestMethod]
    public async Task Should_default_to_last_area()
    {
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        client.DefaultRequestHeaders.Add("Cookie", "redir-user-area=true");

        await GetRootAsync(client, "/planner");
        await GetRootAsync(client, "/planner");

        await GetAsync(client, "/shoppinglist");
        await GetRootAsync(client, "/shoppinglist");

        await GetAsync(client, "/notes");
        await GetRootAsync(client, "/notes");

        await GetAsync(client, "/planner");
        await GetAsync(client, "/meals");
        await GetRootAsync(client, "/meals");

        await GetAsync(client, "/planner");
        await GetRootAsync(client, "/planner");

        static async Task GetRootAsync(HttpClient client, string expectedRedirect)
        {
            using var response = await client.GetAsync("/");
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
            Assert.AreEqual(new Uri(expectedRedirect, UriKind.Relative), response.Headers.Location);
        }

        static async Task GetAsync(HttpClient client, string path)
        {
            using var response = await client.GetAsync(path);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}