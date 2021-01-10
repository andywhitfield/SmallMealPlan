using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmallMealPlan.SmallLister
{
    public interface ISmallListerClient
    {
        Task<SmallListerList> GetListAsync(string refreshToken, string listId);
        Task AddItemAsync(string refreshToken, string listId, string itemToAddToList);
        Task<IEnumerable<SmallListerList>> GetListsAsync(string refreshToken);
    }
}