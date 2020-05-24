using System.Collections.Generic;
using System.Threading.Tasks;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public interface IShoppingListRepository
    {
        Task<ShoppingListItem> GetAsync(int shoppingListItemId);
        Task<List<ShoppingListItem>> GetActiveItemsAsync(UserAccount user);
        Task AddAsync(UserAccount user, string description);
        Task MarkAsBoughtAsync(UserAccount user, ShoppingListItem shoppingListItem);
    }
}