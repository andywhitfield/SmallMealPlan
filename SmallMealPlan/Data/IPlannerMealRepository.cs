using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public interface IPlannerMealRepository
    {
        Task AddMealToPlannerAsync(UserAccount user, DateTime date, Meal meal);
        Task AddNewMealToPlannerAsync(UserAccount user, DateTime date, string description, IEnumerable<string> ingredients, string notes);
        Task<List<PlannerMeal>> GetPlannerMealsAsync(UserAccount user, DateTime fromDateInclusive, DateTime toDateExclusive);
        Task DeleteMealFromPlannerAsync(UserAccount user, int mealPlannerId);
        Task UpdateAsync(UserAccount user, int mealPlannerId, DateTime date, int? sortOrderPreviousPlannerMealId);
    }
}