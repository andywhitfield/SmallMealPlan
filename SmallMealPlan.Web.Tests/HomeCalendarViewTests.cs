using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallMealPlan.Data;
using SmallMealPlan.Web.Controllers;

namespace SmallMealPlan.Web.Tests;

[TestClass]
public class HomeCalendarViewTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestInitialize]
    public async Task SetupAsync()
    {
        var testUser = await _webApplicationFactory.CreateTestUserAsync();
        await using var scope = _webApplicationFactory.Services.CreateAsyncScope();
        using var context = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var user = await context.UserAccounts.SingleAsync(u => u.Email == testUser.Email);
        user.HomeView = HomeController.HomeViewCalendar;
        await context.SaveChangesAsync();
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();

    [TestMethod]
    public async Task Given_user_with_view_as_calendar_Should_show_calendar()
    {
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var plannerResponse = await client.GetAsync("/planner");
        Assert.AreEqual(HttpStatusCode.OK, plannerResponse.StatusCode);
        var responseContent = await plannerResponse.Content.ReadAsStringAsync();
        Assert.Contains("<div id=\"calendar\"></div>", responseContent);
    }

    [TestMethod]
    public async Task Should_switch_to_list_view()
    {
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var plannerResponse = await client.GetAsync("/planner?view=list");
        Assert.AreEqual(HttpStatusCode.OK, plannerResponse.StatusCode);
        var responseContent = await plannerResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<div id=\"calendar\"></div>", responseContent);

        await using var scope = _webApplicationFactory.Services.CreateAsyncScope();
        using var context = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var user = await context.UserAccounts.SingleAsync(u => u.Email == _webApplicationFactory.TestUser.Email);
        Assert.AreEqual("", user.HomeView);
    }

    [TestMethod]
    public async Task Should_switch_back_to_calendar_view()
    {
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var listPlannerResponse = await client.GetAsync("/planner?view=list");
        Assert.AreEqual(HttpStatusCode.OK, listPlannerResponse.StatusCode);
        var responseContent = await listPlannerResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<div id=\"calendar\"></div>", responseContent);

        {
            await using var scope = _webApplicationFactory.Services.CreateAsyncScope();
            using var context = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();
            var user = await context.UserAccounts.SingleAsync(u => u.Email == _webApplicationFactory.TestUser.Email);
            Assert.AreEqual("", user.HomeView);
        }

        using var calendarPlannerResponse = await client.GetAsync("/planner?view=calendar");
        Assert.AreEqual(HttpStatusCode.OK, calendarPlannerResponse.StatusCode);
        responseContent = await calendarPlannerResponse.Content.ReadAsStringAsync();
        Assert.Contains("<div id=\"calendar\"></div>", responseContent);

        {
            await using var scope = _webApplicationFactory.Services.CreateAsyncScope();
            using var context = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();
            var user = await context.UserAccounts.SingleAsync(u => u.Email == _webApplicationFactory.TestUser.Email);
            Assert.AreEqual(HomeController.HomeViewCalendar, user.HomeView);
        }
    }
}