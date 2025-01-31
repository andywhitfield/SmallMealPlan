using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public interface IPlannerMealRepository
{
    Task<PlannerMeal> GetAsync(int plannerMealId);
    Task AddMealToPlannerAsync(UserAccount user, DateTime date, Meal meal);
    Task AddNewMealToPlannerAsync(UserAccount user, DateTime date, string description, IEnumerable<string> ingredients, string? notes, string? dateNotes);
    Task<List<PlannerMeal>> GetPlannerMealsAsync(UserAccount user, DateTime fromDateInclusive, DateTime toDateExclusive);
    Task DeleteMealFromPlannerAsync(UserAccount user, int mealPlannerId);
    Task UpdateAsync(UserAccount user, int mealPlannerId, DateTime date, int? sortOrderPreviousPlannerMealId);
    Task UpdateMealPlannerAsync(UserAccount user, int plannerMealId, DateTime date, string description, IEnumerable<string> ingredients, string? notes, string? dateNotes);
}