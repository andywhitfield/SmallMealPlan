using System.Collections.Generic;
using System.Threading.Tasks;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public interface IShoppingListRepository
    {
        Task<ShoppingListItem> GetAsync(int shoppingListItemId);
        Task<List<ShoppingListItem>> GetActiveItemsAsync(UserAccount user);
        Task<List<(Meal Meal, Ingredient Ingredient)>> GetFutureMealIngredientsFromPlannerAsync(UserAccount user);
        Task<(List<ShoppingListItem> Items, int PageNumber, int PageCount)> GetBoughtItemsAsync(UserAccount user, int pageNumber);
        Task<ShoppingListItem> AddNewIngredientAsync(UserAccount user, string description);
        Task AddIngredientAsync(UserAccount user, int ingredientId);
        Task<List<ShoppingListItem>> AddIngredientsAsync(UserAccount user, params int[] ingredientId);
        Task MarkAsActiveAsync(UserAccount user, ShoppingListItem shoppingListItem);
        Task MarkAsBoughtAsync(UserAccount user, IEnumerable<ShoppingListItem> shoppingListItems);
        Task DeleteAsync(UserAccount user, ShoppingListItem shoppingListItem);
        Task ReorderAsync(UserAccount user, int shoppingListItemId, int? sortOrderPreviousShoppingListItemId);
    }
}