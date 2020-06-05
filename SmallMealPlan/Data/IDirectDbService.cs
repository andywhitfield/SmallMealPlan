using System.Collections.Generic;
using System.Threading.Tasks;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public interface IDirectDbService
    {
        Task<(List<int> MealIds, int PageNumber, int PageCount)> GetMealIdsByMostRecentlyUsedAsync(UserAccount user, int pageNumber, int pageSize, string filter);
        Task<(List<int> MealIds, int PageNumber, int PageCount)> GetMealIdsByNameAsync(UserAccount user, int pageNumber, int pageSize, string filter);
        Task RemovePlannerMealByMealIdAsync(int mealId);
    }
}