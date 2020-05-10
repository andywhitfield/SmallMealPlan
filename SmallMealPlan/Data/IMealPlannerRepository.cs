using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public interface IMealPlannerRepository
    {
        Task AddNewMealToPlannerAsync(UserAccount userAccount, DateTime date, string description, IEnumerable<string> ingredients, string notes);
        Task<List<PlannerMeal>> GetPlannerMealsAsync(UserAccount userAccount, DateTime fromDateInclusive, DateTime toDateExclusive);
        Task DeleteMealFromPlannerAsync(UserAccount userAccount, int mealPlannerId);
    }
}