using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public class ShoppingListRepository : IShoppingListRepository
    {
        private const int BoughtItemsPageSize = 10;

        private readonly SqliteDataContext _context;
        private readonly ILogger<ShoppingListRepository> _logger;

        public ShoppingListRepository(SqliteDataContext context, ILogger<ShoppingListRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ShoppingListItem> GetAsync(int shoppingListItemId)
        {
            var shoppingListItem = await _context.ShoppingListItems.Include(s => s.Ingredient).Where(s => s.ShoppingListItemId == shoppingListItemId).SingleOrDefaultAsync();
            return shoppingListItem ?? throw new ArgumentException($"ShoppingListItemId {shoppingListItemId} not found", nameof(shoppingListItemId));
        }

        public Task<List<ShoppingListItem>> GetActiveItemsAsync(UserAccount user) =>
            _context.ShoppingListItems
                .Include(s => s.Ingredient)
                .Where(s => s.User == user && s.BoughtDateTime == null && s.DeletedDateTime == null)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

        public async Task<List<(Meal Meal, Ingredient Ingredient)>> GetFutureMealIngredientsFromPlannerAsync(UserAccount user) =>
            (await _context
                .PlannerMeals
                .Include(pm => pm.Meal)
                .ThenInclude(m => m.Ingredients)
                .ThenInclude(mi => mi.Ingredient)
                .Where(pm => pm.User == user)
                .Where(pm => pm.Date >= DateTime.UtcNow.Date)
                .Where(pm => pm.DeletedDateTime == null)
                .OrderBy(pm => pm.Date)
                .ThenBy(pm => pm.SortOrder)
                .ToListAsync())
            .SelectMany(pm => pm.Meal.Ingredients)
            .Where(mi => mi.DeletedDateTime == null && mi.Ingredient.DeletedDateTime == null)
            .Select(mi => (mi.Meal, mi.Ingredient))
            .ToList();

        public async Task<(List<ShoppingListItem> Items, int PageNumber, int PageCount)> GetBoughtItemsAsync(UserAccount user, int pageNumber)
        {
            var total = await _context.ShoppingListItems
                .Where(s => s.User == user && s.BoughtDateTime != null && s.DeletedDateTime == null)
                .CountAsync();
            var pagination = Paging.GetPageInfo(total, BoughtItemsPageSize, pageNumber);
            _logger.LogTrace($"Getting page index {pagination.PageIndex} of {pagination.PageCount} total pages, total items: {total}, requested page: {pageNumber}");
            
            return (await _context.ShoppingListItems
                .Include(s => s.Ingredient)
                .Where(s => s.User == user && s.BoughtDateTime != null && s.DeletedDateTime == null)
                .OrderByDescending(s => s.BoughtDateTime)
                .Skip(pagination.PageIndex * BoughtItemsPageSize)
                .Take(BoughtItemsPageSize)
                .ToListAsync(), pagination.PageIndex + 1, pagination.PageCount);
        }

        public async Task AddNewIngredientAsync(UserAccount user, string description)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (description == null) throw new ArgumentNullException(nameof(description));

            var maxSortOrder = await GetMaxSortOrder(user);
            await _context.ShoppingListItems.AddAsync(new ShoppingListItem
            {
                User = user,
                CreatedDateTime = DateTime.UtcNow,
                Ingredient = new Ingredient
                {
                    CreatedBy = user,
                    CreatedDateTime = DateTime.UtcNow,
                    Description = description
                },
                SortOrder = maxSortOrder + 1
            });
            await _context.SaveChangesAsync();
        }

        public async Task AddIngredientAsync(UserAccount user, int ingredientId)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            await InternalAddIngredientAsync(user, ingredientId);
            await _context.SaveChangesAsync();
        }

        public async Task AddIngredientsAsync(UserAccount user, params int[] ingredientIds)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (ingredientIds == null) return;
            foreach (var ingredientId in ingredientIds)
                await InternalAddIngredientAsync(user, ingredientId);
            await _context.SaveChangesAsync();
        }

        private async Task InternalAddIngredientAsync(UserAccount user, int ingredientId)
        {
            var ingredient = (await _context.Ingredients.FindAsync(ingredientId)) ?? throw new ArgumentException($"Ingredient {ingredientId} not found", nameof(ingredientId));
            if (ingredient.DeletedDateTime != null)
                throw new ArgumentException($"Ingredient {ingredientId} not valid", nameof(ingredientId));

            var maxSortOrder = await GetMaxSortOrder(user);
            await _context.ShoppingListItems.AddAsync(new ShoppingListItem
            {
                User = user,
                CreatedDateTime = DateTime.UtcNow,
                Ingredient = ingredient,
                SortOrder = maxSortOrder + 1
            });
        }

        public Task MarkAsBoughtAsync(UserAccount user, ShoppingListItem shoppingListItem)
        {
            if (shoppingListItem.User.UserAccountId != user.UserAccountId)
                throw new SecurityException($"Cannot update shopping list item id: {shoppingListItem.ShoppingListItemId}");

            shoppingListItem.LastUpdateDateTime = shoppingListItem.BoughtDateTime = DateTime.UtcNow;
            return _context.SaveChangesAsync();
        }

        public Task DeleteAsync(UserAccount user, ShoppingListItem shoppingListItem)
        {
            if (shoppingListItem.User.UserAccountId != user.UserAccountId)
                throw new SecurityException($"Cannot delete shopping list item id: {shoppingListItem.ShoppingListItemId}");

            shoppingListItem.DeletedDateTime = DateTime.UtcNow;
            return _context.SaveChangesAsync();
        }

        public async Task MarkAsActiveAsync(UserAccount user, ShoppingListItem shoppingListItem)
        {
            if (shoppingListItem.User.UserAccountId != user.UserAccountId)
                throw new SecurityException($"Cannot update shopping list item id: {shoppingListItem.ShoppingListItemId}");

            shoppingListItem.LastUpdateDateTime = DateTime.UtcNow;
            shoppingListItem.BoughtDateTime = null;
            shoppingListItem.SortOrder = (await GetMaxSortOrder(user)) + 1;
            await _context.SaveChangesAsync();
        }

        public async Task ReorderAsync(UserAccount user, int shoppingListItemId, int? sortOrderPreviousShoppingListItemId)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var shoppingListItem = await _context.ShoppingListItems.FindAsync(shoppingListItemId);
            if (shoppingListItem == null)
                return;

            if (shoppingListItem.User.UserAccountId != user.UserAccountId)
                throw new SecurityException($"Cannot update shopping list item id: {shoppingListItem.ShoppingListItemId}");

            _logger.LogDebug($"Update shopping list item id: {shoppingListItem.ShoppingListItemId} to be after {sortOrderPreviousShoppingListItemId}");

            var shoppingListItems = _context.ShoppingListItems
                .Where(pm => pm.User == user)
                .Where(pm => pm.BoughtDateTime == null)
                .Where(pm => pm.DeletedDateTime == null)
                .OrderBy(pm => pm.SortOrder)
                .AsAsyncEnumerable();

            int? sortOrder = null;
            if (!sortOrderPreviousShoppingListItemId.HasValue)
            {
                sortOrder = 0;
                shoppingListItem.SortOrder = sortOrder.Value;
                sortOrder++;
            }

            ShoppingListItem lastShoppingListItem = null;
            await foreach (var sli in shoppingListItems)
            {
                if (sli.ShoppingListItemId == shoppingListItem.ShoppingListItemId)
                    continue;

                if (sortOrderPreviousShoppingListItemId == sli.ShoppingListItemId)
                {
                    sortOrder = sli.SortOrder + 1;
                    shoppingListItem.SortOrder = sortOrder.Value;
                    sortOrder++;
                }
                else if (sortOrder.HasValue)
                {
                    sli.SortOrder = sortOrder.Value;
                    sortOrder++;
                }

                lastShoppingListItem = sli;
            }

            shoppingListItem.LastUpdateDateTime = DateTime.UtcNow;
            if (!sortOrder.HasValue)
                shoppingListItem.SortOrder = lastShoppingListItem?.SortOrder ?? 0;

            await _context.SaveChangesAsync();
        }

        private async Task<int> GetMaxSortOrder(UserAccount user) =>
            (await _context.ShoppingListItems.Where(s => s.User == user && s.BoughtDateTime == null && s.DeletedDateTime == null).MaxAsync(s => (int?)s.SortOrder)) ?? -1;
    }
}