using System.Collections.Generic;
using System.Threading.Tasks;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public interface IMealRepository
    {
        Task<Meal> GetAsync(int mealId);
        Task<(List<Meal> Meals, int PageNumber, int PageCount)> GetMealsByMostRecentlyUsedAsync(UserAccount user, int pageNumber, string filter);
        Task<(List<Meal> Meals, int PageNumber, int PageCount)> GetMealsByNameAsync(UserAccount user, int pageNumber, string filter);
        Task AddNewMealAsync(UserAccount user, string description, IEnumerable<string> ingredients, string notes);
        Task UpdateMealAsync(UserAccount user, Meal meal, string description, IEnumerable<string> ingredients, string notes);
        Task DeleteMealAsync(UserAccount user, Meal meal);
    }
}