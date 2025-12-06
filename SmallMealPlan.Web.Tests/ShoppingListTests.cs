using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallMealPlan.Data;

namespace SmallMealPlan.Web.Tests;

[TestClass]
public class ShoppingListTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();

    [TestMethod]
    public async Task Given_no_credentials_should_redirect_to_login()
    {
        using var client = await _webApplicationFactory.CreateUnauthenticatedClientAsync();
        using var response = await client.GetAsync("/shoppinglist");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Should_show_shopping_list_items()
    {
        await AddShoppingListItemsAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.GetAsync("/shoppinglist");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(responseContent, "Logout");

        // checking the existance and order of items in the 'my list'
        var idx = responseContent.IndexOf("item 2");
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("item 1", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("item 4", idx);
        Assert.IsGreaterThan(0, idx, responseContent);

        // check the order (and count) of items in the 'regular/bought' list
        // by default, it lists in 'regular' order, so only expect one entry per ingredient
        // description, therefore should be 3 items in the list in the order item 3, 5, then 6.
        idx = responseContent.IndexOf("item 3");
        Assert.IsGreaterThan(0, idx, responseContent);
        Assert.IsLessThan(0, responseContent.IndexOf("item 3", idx + 1), responseContent);
        idx = responseContent.IndexOf("item 5", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
        Assert.IsLessThan(0, responseContent.IndexOf("item 5", idx + 1), responseContent);
        idx = responseContent.IndexOf("item 6", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
        Assert.IsLessThan(0, responseContent.IndexOf("item 6", idx + 1), responseContent);
    }

    [TestMethod]
    public async Task Should_show_shopping_list_items_and_all_bought_items()
    {
        await AddShoppingListItemsAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.GetAsync("/shoppinglist?regularOrBought=bought");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(responseContent, "Logout");

        // checking the existance and order of items in the 'my list'
        var idx = responseContent.IndexOf("item 2");
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("item 1", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("item 4", idx);
        Assert.IsGreaterThan(0, idx, responseContent);

        // check the order (and count) of items in the 'regular/bought' list
        // should be in bought order: item 6, 3, 5, 3, 5
        idx = responseContent.IndexOf("item 6");
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("item 3", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("item 5", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("item 3", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("item 5", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
    }

    private async Task AddShoppingListItemsAsync()
    {
        await _webApplicationFactory.CreateTestUserAsync();
        await using var services = _webApplicationFactory.Services.CreateAsyncScope();
        var context = services.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var user = await context.UserAccounts.SingleAsync();
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 1" }, SortOrder = 2 });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 2" }, SortOrder = 1 });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 3" }, BoughtDateTime = DateTime.UtcNow.AddDays(-2) });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 4" }, SortOrder = 3 });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 5" }, BoughtDateTime = DateTime.UtcNow.AddDays(-1) });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 3" }, BoughtDateTime = DateTime.UtcNow.AddMinutes(-1) });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 5" }, BoughtDateTime = DateTime.UtcNow.AddDays(-3) });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 6" }, BoughtDateTime = DateTime.UtcNow });
        await context.SaveChangesAsync();
    }
}