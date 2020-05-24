using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public class ShoppingListRepository : IShoppingListRepository
    {
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

        public async Task AddAsync(UserAccount user, string description)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (description == null) throw new ArgumentNullException(nameof(description));

            var maxSortOrder = (await _context.ShoppingListItems.Where(s => s.User == user && s.BoughtDateTime == null && s.DeletedDateTime == null).MaxAsync(s => (int?)s.SortOrder)) ?? 0;
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

        public Task MarkAsBoughtAsync(UserAccount user, ShoppingListItem shoppingListItem)
        {
            shoppingListItem.LastUpdateDateTime = shoppingListItem.BoughtDateTime = DateTime.UtcNow;
            return _context.SaveChangesAsync();
        }
    }
}