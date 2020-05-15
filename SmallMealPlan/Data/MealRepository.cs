using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public class MealRepository : IMealRepository
    {
        private const int PageSize = 10;

        private readonly SqliteDataContext _context;
        private readonly ILogger<MealRepository> _logger;
        private readonly IDirectQueryService _directQueryService;

        public MealRepository(SqliteDataContext context, ILogger<MealRepository> logger,
            IDirectQueryService directQueryService)
        {
            _context = context;
            _logger = logger;
            _directQueryService = directQueryService;
        }

        public async Task<Meal> GetAsync(int mealId)
        {
            var meal = await _context.Meals.FindAsync(mealId);
            return meal ?? throw new ArgumentException($"MealId {mealId} not found", nameof(mealId));
        }

        public async Task<(List<Meal> Meals, int PageNumber, int PageCount)> GetMealsByMostRecentlyUsedAsync(UserAccount user, int pageNumber)
        {
            var mealIds = await _directQueryService.GetMealIdsByMostRecentlyUsedAsync(user, pageNumber, PageSize);
            return ((
                await _context
                    .Meals
                    .Include(m => m.Ingredients)
                    .ThenInclude(mi => mi.Ingredient)
                    .Where(m => m.User == user)
                    .Where(m => mealIds.MealIds.Contains(m.MealId))
                    .ToListAsync())
                .OrderBy(m => mealIds.MealIds.IndexOf(m.MealId))
                .ToList(), mealIds.PageNumber, mealIds.PageCount);
        }

        public async Task<(List<Meal> Meals, int PageNumber, int PageCount)> GetMealsByNameAsync(UserAccount user, int pageNumber)
        {
            var mealIds = await _directQueryService.GetMealIdsByNameAsync(user, pageNumber, PageSize);
            return ((
                await _context
                    .Meals
                    .Include(m => m.Ingredients)
                    .ThenInclude(mi => mi.Ingredient)
                    .Where(m => m.User == user)
                    .Where(m => mealIds.MealIds.Contains(m.MealId))
                    .ToListAsync())
                .OrderBy(m => mealIds.MealIds.IndexOf(m.MealId))
                .ToList(), mealIds.PageNumber, mealIds.PageCount);
        }
    }
}