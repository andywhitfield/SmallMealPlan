using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallMealPlan.Data;
using SmallMealPlan.Model;
using SmallMealPlan.Web.Model.Meals;
using SmallMealPlan.Web.Model.Request;

namespace SmallMealPlan.Web.Controllers;

[Authorize]
public class MealsController(
    IUserAccountRepository userAccountRepository,
    IMealRepository mealRepository)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] int? pageNumber, [FromQuery] string? sort, [FromQuery] string? filter)
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        List<Meal> meals;
        int page;
        int pageCount;
        var sortByName = sort == Pagination.SortByName;
        if (sortByName)
            (meals, page, pageCount) = await mealRepository.GetMealsByNameAsync(user, pageNumber ?? 1, filter);
        else
            (meals, page, pageCount) = await mealRepository.GetMealsByMostRecentlyUsedAsync(user, pageNumber ?? 1, filter);

        return View(new IndexViewModel(HttpContext)
        {
            Pagination = new Pagination(page, pageCount, sortByName ? Pagination.SortByName : Pagination.SortByRecentlyUsed, filter),
            Meals = meals
            .Select(m => new Model.Home.PlannerDayMealViewModel(m.MealId)
            {
                Name = m.Description,
                Notes = m.Notes,
                Ingredients = m.Ingredients?.OrderBy(mi => mi.SortOrder).Select(i => i.Ingredient.Description?.Trim() ?? "") ?? []
            })
            .ToList()
        });
    }

    [HttpPost("~/meals")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNewMeal([FromForm] AddMealRequest addModel)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        var user = await userAccountRepository.GetUserAccountAsync(User);
        await mealRepository.AddNewMealAsync(user, addModel.Description.Trim(), addModel.Ingredients?.Split('\n', StringSplitOptions.TrimEntries).Where(i => !string.IsNullOrWhiteSpace(i)) ?? [], addModel.Notes?.Trim());
        return Redirect("~/meals");
    }

    [HttpGet("~/meal/edit/{mealId}")]
    public async Task<IActionResult> Edit([FromRoute] int mealId)
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        var meal = await mealRepository.GetAsync(mealId);
        if (meal.User != user)
            return BadRequest();

        return View(new EditMealViewModel(HttpContext, meal.MealId)
        {
            Name = meal.Description,
            Ingredients = meal.Ingredients == null ? null : string.Join('\n', meal.Ingredients.OrderBy(mi => mi.SortOrder).Select(mi => mi.Ingredient.Description?.Trim() ?? "")),
            Notes = meal.Notes
        });
    }

    [HttpPost("~/meal/edit/{mealId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEdit([FromRoute] int mealId, [FromForm] EditPlannerMealRequest editModel)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        if (editModel.Cancel ?? false)
            return Redirect($"~/meals");

        var user = await userAccountRepository.GetUserAccountAsync(User);
        var meal = await mealRepository.GetAsync(mealId);
        if (meal.User != user)
            return BadRequest();

        var ingredients = editModel.Ingredients?.Split('\n', StringSplitOptions.TrimEntries).Where(i => !string.IsNullOrWhiteSpace(i)) ?? [];

        if (editModel.SaveAsNew ?? false)
            await mealRepository.AddNewMealAsync(user, editModel.Description.Trim(), ingredients, editModel.Notes?.Trim());
        else
            await mealRepository.UpdateMealAsync(user, meal, editModel.Description.Trim(), ingredients, editModel.Notes?.Trim());

        return Redirect($"~/meals");
    }

    [HttpPost("~/meal/delete/{mealId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMeal([FromRoute] int mealId)
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        var meal = await mealRepository.GetAsync(mealId);
        await mealRepository.DeleteMealAsync(user, meal);
        return Redirect("~/meals");
    }
}