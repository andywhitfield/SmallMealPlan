using System.Collections.Generic;
using System.Threading.Tasks;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public interface IDirectQueryService
    {
        Task<(List<int> MealIds, int PageNumber, int PageCount)> GetMealIdsByMostRecentlyUsedAsync(UserAccount user, int pageNumber, int pageSize);
        Task<(List<int> MealIds, int PageNumber, int PageCount)> GetMealIdsByNameAsync(UserAccount user, int pageNumber, int pageSize);
    }
}