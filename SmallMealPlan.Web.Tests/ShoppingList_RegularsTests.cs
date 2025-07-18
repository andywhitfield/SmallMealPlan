using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallMealPlan.Data;

namespace SmallMealPlan.Web.Tests;

[TestClass]
public class ShoppingList_RegularsTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();

    [TestMethod]
    public async Task Should_show_shopping_list_items_and_regular_items()
    {
        await AddShoppingListItemsAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.GetAsync("/shoppinglist?regularOrBought=regular");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(responseContent, "Logout");

        // check 'my list'
        var myListItems = responseContent[responseContent.IndexOf("class=\"smp-shoplist-current\"")..];
        myListItems = myListItems[..myListItems.IndexOf("class=\"smp-shoplist-add-from-planner\"")];
        Assert.AreEqual(4, myListItems.Split("data-shoppinglistitem").Length - 1);
        var idx = myListItems.IndexOf("item 2");
        Assert.IsTrue(idx > 0, myListItems);
        idx = myListItems.IndexOf("item 1", idx);
        Assert.IsTrue(idx > 0, myListItems);
        idx = myListItems.IndexOf("item 4", idx);
        Assert.IsTrue(idx > 0, myListItems);
        idx = myListItems.IndexOf("ITEM 5", idx);
        Assert.IsTrue(idx > 0, myListItems);

        // check 'regulars' list
        var regularItems = responseContent[responseContent.IndexOf("class=\"smp-shoplist-bought\"")..];
        regularItems = regularItems[..regularItems.IndexOf("class=\"smp-shoplist-sync\"")];
        Assert.AreEqual(2, regularItems.Split("smp-shoplist-bought-date").Length - 1);
        idx = responseContent.IndexOf("item 6");
        Assert.IsTrue(idx > 0, responseContent);
        idx = responseContent.IndexOf("item 3", idx);
        Assert.IsTrue(idx > 0, responseContent);
    }

    [TestMethod]
    public async Task When_show_all_Then_current_shopping_list_items_should_be_on_regulars_page()
    {
        await AddShoppingListItemsAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.GetAsync("/shoppinglist?regularOrBought=regular&shoplistBoughtShowAll=true");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(responseContent, "Logout");

        // check 'my list'
        var myListItems = responseContent[responseContent.IndexOf("class=\"smp-shoplist-current\"")..];
        myListItems = myListItems[..myListItems.IndexOf("class=\"smp-shoplist-add-from-planner\"")];
        Assert.AreEqual(4, myListItems.Split("data-shoppinglistitem").Length - 1);
        var idx = myListItems.IndexOf("item 2");
        Assert.IsTrue(idx > 0, myListItems);
        idx = myListItems.IndexOf("item 1", idx);
        Assert.IsTrue(idx > 0, myListItems);
        idx = myListItems.IndexOf("item 4", idx);
        Assert.IsTrue(idx > 0, myListItems);
        idx = myListItems.IndexOf("ITEM 5", idx);
        Assert.IsTrue(idx > 0, myListItems);

        // check 'regulars' list
        var regularItems = responseContent[responseContent.IndexOf("class=\"smp-shoplist-bought\"")..];
        regularItems = regularItems[..regularItems.IndexOf("class=\"smp-shoplist-sync\"")];
        Assert.AreEqual(3, regularItems.Split("smp-shoplist-bought-date").Length - 1);
        idx = responseContent.IndexOf("item 6");
        Assert.IsTrue(idx > 0, responseContent);
        idx = responseContent.IndexOf("item 3", idx);
        Assert.IsTrue(idx > 0, responseContent);
        idx = responseContent.IndexOf("item 5", idx);
        Assert.IsTrue(idx > 0, responseContent);
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
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "ITEM 5" }, SortOrder = 4 });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 5" }, BoughtDateTime = DateTime.UtcNow.AddDays(-1) });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 3" }, BoughtDateTime = DateTime.UtcNow.AddMinutes(-1) });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 3" }, BoughtDateTime = DateTime.UtcNow.AddMinutes(-1) });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 5" }, BoughtDateTime = DateTime.UtcNow.AddDays(-3) });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 6" }, BoughtDateTime = DateTime.UtcNow.AddMinutes(-2) });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 6" }, BoughtDateTime = DateTime.UtcNow.AddMinutes(-3) });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 6" }, BoughtDateTime = DateTime.UtcNow.AddMinutes(-4) });
        context.ShoppingListItems.Add(new() { User = user, Ingredient = new() { Description = "item 6" }, BoughtDateTime = DateTime.UtcNow.AddMinutes(-5) });
        await context.SaveChangesAsync();
    }
}