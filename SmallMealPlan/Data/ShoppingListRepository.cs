using System.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public class ShoppingListRepository(SqliteDataContext context, ILogger<ShoppingListRepository> logger)
    : IShoppingListRepository
{
    private const int BoughtItemsPageSize = 10;

    public async Task<ShoppingListItem> GetAsync(int shoppingListItemId)
    {
        var shoppingListItem = await context
            .ShoppingListItems
            .Include(s => s.Ingredient)
            .Where(s => s.ShoppingListItemId == shoppingListItemId)
            .SingleOrDefaultAsync();
        return shoppingListItem ?? throw new ArgumentException($"ShoppingListItemId {shoppingListItemId} not found", nameof(shoppingListItemId));
    }

    public Task<List<ShoppingListItem>> GetActiveItemsAsync(UserAccount user) =>
        context
            .ShoppingListItems
            .Include(s => s.Ingredient)
            .Where(s => s.User == user && s.BoughtDateTime == null && s.DeletedDateTime == null)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();

    public async Task<List<(Meal Meal, Ingredient Ingredient)>> GetFutureMealIngredientsFromPlannerAsync(UserAccount user) =>
        [.. (await context
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
        .Select(mi => (mi.Meal ?? throw new InvalidOperationException($"Could not get meal from {mi.MealIngredientId}"), mi.Ingredient))
        .Distinct()];

    public async Task<(List<ShoppingListItem> Items, int PageNumber, int PageCount)> GetBoughtItemsAsync(UserAccount user, int pageNumber)
    {
        var total = await context
            .ShoppingListItems
            .Where(s => s.User == user && s.BoughtDateTime != null && s.DeletedDateTime == null)
            .CountAsync();
        var (pageIndex, pageCount) = Paging.GetPageInfo(total, BoughtItemsPageSize, pageNumber);
        logger.LogTrace("Getting page index {PageIndex} of {PageCount} total pages, total items: {Total}, requested page: {PageNumber}", pageIndex, pageCount, total, pageNumber);

        return (await context
            .ShoppingListItems
            .Include(s => s.Ingredient)
            .Where(s => s.User == user && s.BoughtDateTime != null && s.DeletedDateTime == null)
            .OrderByDescending(s => s.BoughtDateTime)
            .Skip(pageIndex * BoughtItemsPageSize)
            .Take(BoughtItemsPageSize)
            .ToListAsync(), pageIndex + 1, pageCount);
    }

    public async Task<(List<ShoppingListItem> Items, int PageNumber, int PageCount)> GetRegularItemsAsync(UserAccount user, int pageNumber)
    {
        var total = await context
            .ShoppingListItems
            .Include(s => s.Ingredient)
            .Where(s => s.User == user && s.BoughtDateTime != null && s.DeletedDateTime == null)
            .Select(s => s.Ingredient.Description)
            .Distinct()
            .CountAsync();
        var (pageIndex, pageCount) = Paging.GetPageInfo(total, BoughtItemsPageSize, pageNumber);
        logger.LogTrace("Getting page index {PageIndex} of {PageCount} total pages, total items: {Total}, requested page: {PageNumber}", pageIndex, pageCount, total, pageNumber);

        var incredientByCount = context
            .ShoppingListItems
            .Include(s => s.Ingredient)
            .Where(s => s.User == user && s.BoughtDateTime != null && s.DeletedDateTime == null)
            .GroupBy(s => s.Ingredient.Description)
            .Select(g => new { IngredientDescription = g.Key, Count = g.Count(), LatestShoppingListItemId = g.Max(i => i.ShoppingListItemId) });

        return (await context
            .ShoppingListItems
            .Include(s => s.Ingredient)
            .Join(incredientByCount, s => s.ShoppingListItemId, g => g.LatestShoppingListItemId, (s, g) => new { ShoppingListItem = s, g.Count })
            .OrderByDescending(s => s.Count)
            .ThenByDescending(s => s.ShoppingListItem.BoughtDateTime)
            .Select(s => s.ShoppingListItem)
            .Skip(pageIndex * BoughtItemsPageSize)
            .Take(BoughtItemsPageSize)
            .ToListAsync(), pageIndex + 1, pageCount);
    }

    public async Task<ShoppingListItem> AddNewIngredientAsync(UserAccount user, string description)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(description);

        var maxSortOrder = await GetMaxSortOrder(user);
        var item = await context.ShoppingListItems.AddAsync(new ShoppingListItem
        {
            User = user,
            CreatedDateTime = DateTime.UtcNow,
            Ingredient = new()
            {
                CreatedBy = user,
                CreatedDateTime = DateTime.UtcNow,
                Description = description
            },
            SortOrder = maxSortOrder + 1
        });
        await context.SaveChangesAsync();
        return item.Entity;
    }

    public async Task AddIngredientAsync(UserAccount user, int ingredientId)
    {
        ArgumentNullException.ThrowIfNull(user);
        await InternalAddIngredientAsync(user, ingredientId);
        await context.SaveChangesAsync();
    }

    public async Task<List<ShoppingListItem>> AddIngredientsAsync(UserAccount user, params int[] ingredientIds)
    {
        ArgumentNullException.ThrowIfNull(user);
        List<ShoppingListItem> added = [];
        if (ingredientIds == null)
            return added;
        foreach (var ingredientId in ingredientIds)
            added.Add(await InternalAddIngredientAsync(user, ingredientId));
        await context.SaveChangesAsync();
        return added;
    }

    private async Task<ShoppingListItem> InternalAddIngredientAsync(UserAccount user, int ingredientId)
    {
        var ingredient = (await context.Ingredients.FindAsync(ingredientId)) ?? throw new ArgumentException($"Ingredient {ingredientId} not found", nameof(ingredientId));
        if (ingredient.DeletedDateTime != null)
            throw new ArgumentException($"Ingredient {ingredientId} not valid", nameof(ingredientId));

        var maxSortOrder = await GetMaxSortOrder(user);
        return (await context.ShoppingListItems.AddAsync(new ShoppingListItem
        {
            User = user,
            CreatedDateTime = DateTime.UtcNow,
            Ingredient = ingredient,
            SortOrder = maxSortOrder + 1
        })).Entity;
    }

    public Task MarkAsBoughtAsync(UserAccount user, IEnumerable<ShoppingListItem> shoppingListItems)
    {
        if (shoppingListItems.Any(sli => sli.User.UserAccountId != user.UserAccountId))
            throw new SecurityException($"Cannot update shopping list item where the item's owner is not the specifid user");

        var now = DateTime.UtcNow;
        foreach (var shoppingListItem in shoppingListItems)
            shoppingListItem.LastUpdateDateTime = shoppingListItem.BoughtDateTime = now;

        return context.SaveChangesAsync();
    }

    public Task DeleteAsync(UserAccount user, ShoppingListItem shoppingListItem)
    {
        if (shoppingListItem.User.UserAccountId != user.UserAccountId)
            throw new SecurityException($"Cannot delete shopping list item id: {shoppingListItem.ShoppingListItemId}");

        shoppingListItem.DeletedDateTime = DateTime.UtcNow;
        return context.SaveChangesAsync();
    }

    public async Task ReorderAsync(UserAccount user, int shoppingListItemId, int? sortOrderPreviousShoppingListItemId)
    {
        ArgumentNullException.ThrowIfNull(user);

        var shoppingListItem = await context.ShoppingListItems.FindAsync(shoppingListItemId);
        if (shoppingListItem == null)
            return;

        if (shoppingListItem.User.UserAccountId != user.UserAccountId)
            throw new SecurityException($"Cannot update shopping list item id: {shoppingListItem.ShoppingListItemId}");

        logger.LogDebug("Update shopping list item id: {ShoppingListItemId} to be after {SortOrderPreviousShoppingListItemId}", shoppingListItem.ShoppingListItemId, sortOrderPreviousShoppingListItemId);

        var shoppingListItems = context
            .ShoppingListItems
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

        ShoppingListItem? lastShoppingListItem = null;
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

        await context.SaveChangesAsync();
    }

    private async Task<int> GetMaxSortOrder(UserAccount user) =>
        (await context
            .ShoppingListItems
            .Where(s => s.User == user && s.BoughtDateTime == null && s.DeletedDateTime == null)
            .MaxAsync(s => (int?)s.SortOrder)) ?? -1;
}