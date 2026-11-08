using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallMealPlan.Data;
using SmallMealPlan.Web.Model.Home;

namespace SmallMealPlan.Web.Tests;

[TestClass]
public class HomeApiControllerTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestInitialize]
    public async Task Setup()
    {
        await using var scope = _webApplicationFactory.Services.CreateAsyncScope();
        using var context = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var testUser = await _webApplicationFactory.CreateTestUserAsync();
        context.Attach(testUser);

        var ingredient1 = context.Ingredients.Add(new() { Description = "test ingredient 1" });
        var ingredient2 = context.Ingredients.Add(new() { Description = "test ingredient 2" });
        var ingredient3 = context.Ingredients.Add(new() { Description = "test ingredient 3" });
        var ingredient4 = context.Ingredients.Add(new() { Description = "test ingredient 4" });

        var mealIngredient1 = context.MealIngredients.Add(new() { Ingredient = ingredient1.Entity });
        var mealIngredient2 = context.MealIngredients.Add(new() { Ingredient = ingredient2.Entity });
        var mealIngredient3 = context.MealIngredients.Add(new() { Ingredient = ingredient3.Entity });
        var mealIngredient4 = context.MealIngredients.Add(new() { Ingredient = ingredient4.Entity });

        context.Meals.Add(new()
        {
            User = testUser,
            Description = "test meal 1"
        });
        context.Meals.Add(new()
        {
            User = testUser,
            Description = "test meal 2",
            Ingredients = [mealIngredient1.Entity, mealIngredient2.Entity]
        });
        context.Meals.Add(new()
        {
            User = testUser,
            Description = "test meal 3",
            Ingredients = [mealIngredient3.Entity],
            Notes = "test meal 3 notes"
        });
        context.Meals.Add(new()
        {
            User = testUser,
            Description = "test meal 4",
            Ingredients = [mealIngredient1.Entity, mealIngredient4.Entity],
            Notes = "test meal 4 notes"
        });

        await context.SaveChangesAsync();
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();

    [TestMethod]
    public async Task Should_get_meals_for_date()
    {
        await CreatePlannerMealAsync("test meal 1", new(2026, 8, 1));
        await CreatePlannerMealAsync("test meal 2", new(2026, 8, 31));
        await CreatePlannerMealAsync("test meal 3", new(2026, 9, 1));
        await CreatePlannerMealAsync("test meal 4", new(2026, 9, 30), "end of sep");
        await CreatePlannerMealAsync("test meal 3", new(2026, 10, 1));
        await CreatePlannerMealAsync("test meal 2", new(2026, 10, 31));
        await CreatePlannerMealAsync("test meal 1", new(2026, 11, 1), "start of nov");
        await CreatePlannerMealAsync("test meal 2", new(2026, 11, 30));
        await CreatePlannerMealAsync("test meal 3", new(2026, 12, 1));
        await CreatePlannerMealAsync("test meal 4", new(2026, 12, 31), "end of dec");
        await CreatePlannerMealAsync("test meal 3", new(2027, 1, 1));
        await CreatePlannerMealAsync("test meal 2", new(2027, 1, 31));

        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var apiResponse = await client.GetAsync("/api/calendar/meals/2026-11-05");
        Assert.AreEqual(HttpStatusCode.OK, apiResponse.StatusCode);

        var apiModel = await apiResponse.Content.ReadFromJsonAsync<MealApiModel[]>();
        Assert.IsNotNull(apiModel);
        Assert.HasCount(6, apiModel);
        AssertMeal(apiModel[0], "test meal 3", ["test ingredient 3"], new(2026, 10, 1), "Thursday 01 Oct", "", "test meal 3 notes");
        AssertMeal(apiModel[1], "test meal 2", ["test ingredient 2"], new(2026, 10, 31), "Saturday 31 Oct", "", "");
        AssertMeal(apiModel[2], "test meal 1", [], new(2026, 11, 1), "Sunday 01 Nov", "start of nov", "");
        AssertMeal(apiModel[3], "test meal 2", ["test ingredient 2"], new(2026, 11, 30), "Monday 30 Nov", "", "");
        AssertMeal(apiModel[4], "test meal 3", ["test ingredient 3"], new(2026, 12, 1), "Tuesday 01 Dec", "", "test meal 3 notes");
        AssertMeal(apiModel[5], "test meal 4", ["test ingredient 1", "test ingredient 4"], new(2026, 12, 31), "Thursday 31 Dec", "end of dec", "test meal 4 notes");
    }

    private static void AssertMeal(MealApiModel model, string expectedTitle, string[] expectedIngredients,
        DateOnly expectedDate, string expectedDateDesc, string expectedDateNotes, string expectedNotes)
    {
        Assert.AreEqual(expectedTitle, model.Title);
        Assert.AreSequenceEqual(expectedIngredients, model.Ingredients);
        Assert.AreEqual(expectedDate, model.Date);
        Assert.AreEqual(expectedDateDesc, model.DateDesc);
        Assert.AreEqual(expectedDateNotes, model.DateNotes);
        Assert.AreEqual(expectedNotes, model.Notes);
    }

    private async Task CreatePlannerMealAsync(string mealDescription, DateOnly date, string? notesForDate = null)
    {
        await using var scope = _webApplicationFactory.Services.CreateAsyncScope();
        using var context = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var testUser = await _webApplicationFactory.CreateTestUserAsync();
        context.Attach(testUser);
        var meal = await context.Meals.SingleAsync(m => m.Description == mealDescription);
        context.PlannerMeals.Add(new() { User = testUser, Meal = meal, Date = date.ToDateTime(default), Notes = notesForDate });
        await context.SaveChangesAsync();
    }
}