using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallMealPlan.Data;
using SmallMealPlan.Web.Model.Request;

namespace SmallMealPlan.Web.Controllers;

[ApiController]
[Authorize]
public class HomeApiController(
    ILogger<HomeApiController> logger,
    IUserAccountRepository userAccountRepository,
    IPlannerMealRepository plannerMealRepository)
    : ControllerBase
{
    [HttpPut("~/api/planner/{plannerMealId}/move")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Move(int plannerMealId, PlannerMealMoveRequest plannerMealMoveRequest)
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        await plannerMealRepository.UpdateAsync(user, plannerMealId, plannerMealMoveRequest.Date.ParseDateOrToday(), plannerMealMoveRequest.SortOrderPreviousPlannerMealId);
        return NoContent();
    }

    [HttpGet("~/api/calendar/meals/{date}")]
    [ProducesResponseType<IEnumerable<MealApiModel>>(StatusCodes.Status200OK)]
    public async IAsyncEnumerable<MealApiModel> MealsForDate(DateTime date)
    {
        DateOnly dateMonth = new(date.Year, date.Month, 1);
        var fromDate = dateMonth.AddMonths(-1);
        var toDate = dateMonth.AddMonths(2);

        logger.LogInformation("Getting meals for date {Date}, from={FromDate}, to={ToDate}", date, fromDate, toDate);

        var user = await userAccountRepository.GetUserAccountAsync(User);
        foreach (var plannerMeal in await plannerMealRepository.GetPlannerMealsAsync(user, fromDate.ToDateTime(default), toDate.ToDateTime(default)))
            yield return new(plannerMeal.PlannerMealId, DateOnly.FromDateTime(plannerMeal.Date), plannerMeal.Date.ToString("dddd dd MMM"), plannerMeal.Meal.Description, [.. plannerMeal.Meal.Ingredients.Select(i => i.Ingredient.Description)], plannerMeal.Meal.Notes ?? "", plannerMeal.Notes ?? "");
    }
}

public record MealApiModel(int Id, DateOnly Date, string DateDesc, string Title, string[] Ingredients, string Notes, string DateNotes);