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
    public class MealRepository : IMealRepository
    {
        private const int PageSize = 10;

        private readonly SqliteDataContext _context;
        private readonly ILogger<MealRepository> _logger;
        private readonly IDirectDbService _directDbService;

        public MealRepository(SqliteDataContext context, ILogger<MealRepository> logger,
            IDirectDbService directDbService)
        {
            _context = context;
            _logger = logger;
            _directDbService = directDbService;
        }

        public async Task<Meal> GetAsync(int mealId)
        {
            var meal = await _context.Meals.Include(m => m.Ingredients).ThenInclude(mi => mi.Ingredient).Where(m => m.MealId == mealId).SingleOrDefaultAsync();
            return meal ?? throw new ArgumentException($"MealId {mealId} not found", nameof(mealId));
        }

        public async Task<(List<Meal> Meals, int PageNumber, int PageCount)> GetMealsByMostRecentlyUsedAsync(UserAccount user, int pageNumber)
        {
            var mealIds = await _directDbService.GetMealIdsByMostRecentlyUsedAsync(user, pageNumber, PageSize);
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
            var mealIds = await _directDbService.GetMealIdsByNameAsync(user, pageNumber, PageSize);
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

        public async Task AddNewMealAsync(UserAccount user, string description, IEnumerable<string> ingredients, string notes)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            _context.Meals.Add(new Meal
            {
                Description = description,
                Notes = notes,
                User = user,
                Ingredients = ingredients.Select((i, idx) => new MealIngredient
                {
                    Ingredient = new Ingredient { Description = i, CreatedBy = user },
                    SortOrder = idx
                }).ToList()
            });
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMealAsync(UserAccount user, Meal meal, string description, IEnumerable<string> ingredients, string notes)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (meal == null)
                throw new ArgumentNullException(nameof(meal));

            if (meal.User.UserAccountId != user.UserAccountId)
                throw new SecurityException($"Cannot update meal id: {meal.MealId}");

            meal.LastUpdateDateTime = DateTime.UtcNow;
            meal.Description = description;
            meal.Notes = notes;

            if (
                meal.Ingredients?.Count != ingredients.Count() ||
                !meal.Ingredients.Select(mi => mi.Ingredient.Description).SequenceEqual(ingredients))
            {
                // for now, just delete the MealIngredient and create a a new set
                if (meal.Ingredients?.Any() ?? false)
                    _context.MealIngredients.RemoveRange(meal.Ingredients);
                if (ingredients.Any())
                {
                    meal.Ingredients = ingredients.Select((i, idx) => new MealIngredient
                    {
                        Ingredient = new Ingredient { Description = i, CreatedBy = user },
                        SortOrder = idx
                    }).ToList();
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMealAsync(UserAccount user, Meal meal)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (meal == null)
                throw new ArgumentNullException(nameof(meal));

            if (meal.User.UserAccountId != user.UserAccountId)
                throw new SecurityException($"Cannot delete meal id: {meal.MealId}");

            await _directDbService.RemovePlannerMealByMealIdAsync(meal.MealId);
            _context.Meals.Remove(meal);
            await _context.SaveChangesAsync();
        }
    }
}