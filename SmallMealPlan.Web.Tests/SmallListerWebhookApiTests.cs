using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmallMealPlan.Data;
using SmallMealPlan.SmallLister.Webhook;

namespace SmallMealPlan.Web.Tests;

[TestClass]
public class SmallListerWebhookApiTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();

    [TestMethod]
    public async Task Handle_item_change_webhook()
    {
        var userId = await SetupUserAndShoppingListItemsAsync();
        _webApplicationFactory.SmallListerClientMock
            .Setup(x => x.GetListAsync("test-user-token", "2"))
            .ReturnsAsync(new SmallLister.SmallListerList { Items = [
                new SmallLister.SmallListerItem { Description = "item 1" },
                new SmallLister.SmallListerItem { Description = "item 3" }
            ] });
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.PostAsync($"/api/webhook/{userId}/smalllister/listitem", new StringContent(JsonSerializer.Serialize<IEnumerable<ListItemChange>>([new ListItemChange { ListId = "2" }]), Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        await using var services = _webApplicationFactory.Services.CreateAsyncScope();
        var context = services.ServiceProvider.GetRequiredService<SqliteDataContext>();
        Assert.AreEqual(5, await context.ShoppingListItems.CountAsync());
        Assert.IsNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 1")).BoughtDateTime);
        Assert.IsNotNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 2")).BoughtDateTime);
        Assert.IsNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 3")).BoughtDateTime);
        Assert.IsNotNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 4")).BoughtDateTime);
        Assert.IsNotNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 5")).BoughtDateTime);
    }

    [TestMethod]
    public async Task Ignore_item_change_on_other_list()
    {
        var userId = await SetupUserAndShoppingListItemsAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.PostAsync($"/api/webhook/{userId}/smalllister/listitem", new StringContent(JsonSerializer.Serialize<IEnumerable<ListItemChange>>([new ListItemChange { ListId = "1" }]), Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        _webApplicationFactory.SmallListerClientMock.Verify(x => x.GetListAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        await using var services = _webApplicationFactory.Services.CreateAsyncScope();
        var context = services.ServiceProvider.GetRequiredService<SqliteDataContext>();
        Assert.AreEqual(5, await context.ShoppingListItems.CountAsync());
        Assert.IsNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 1")).BoughtDateTime);
        Assert.IsNotNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 2")).BoughtDateTime);
        Assert.IsNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 3")).BoughtDateTime);
        Assert.IsNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 4")).BoughtDateTime);
        Assert.IsNotNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 5")).BoughtDateTime);
    }

    [TestMethod]
    public async Task Handle_item_changing_list()
    {
        var userId = await SetupUserAndShoppingListItemsAsync();
        _webApplicationFactory.SmallListerClientMock
            .Setup(x => x.GetListAsync("test-user-token", "2"))
            .ReturnsAsync(new SmallLister.SmallListerList { Items = [
                new SmallLister.SmallListerItem { Description = "item 3" }
            ] });
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.PostAsync($"/api/webhook/{userId}/smalllister/listitem", new StringContent(JsonSerializer.Serialize<IEnumerable<ListItemChange>>([new ListItemChange { ListId = "3", PreviousListId = "2" }]), Encoding.UTF8, "application/json"));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        await using var services = _webApplicationFactory.Services.CreateAsyncScope();
        var context = services.ServiceProvider.GetRequiredService<SqliteDataContext>();
        Assert.AreEqual(5, await context.ShoppingListItems.CountAsync());
        Assert.IsNotNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 1")).BoughtDateTime);
        Assert.IsNotNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 2")).BoughtDateTime);
        Assert.IsNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 3")).BoughtDateTime);
        Assert.IsNotNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 4")).BoughtDateTime);
        Assert.IsNotNull((await context.ShoppingListItems.SingleAsync(x => x.Ingredient.Description == "item 5")).BoughtDateTime);
    }

    private async Task<int> SetupUserAndShoppingListItemsAsync()
    {
        await _webApplicationFactory.CreateTestUserAsync();
        await using var services = _webApplicationFactory.Services.CreateAsyncScope();
        var context = services.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var user = await context.UserAccounts.SingleAsync();
        user.SmallListerToken = "test-user-token";
        user.SmallListerSyncListId = "2";
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 1" } });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 2" }, BoughtDateTime = DateTime.UtcNow.AddDays(-2) });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 3" } });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 4" } });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 5" }, BoughtDateTime = DateTime.UtcNow.AddDays(-2) });
        await context.SaveChangesAsync();
        return user.UserAccountId;
    }
}