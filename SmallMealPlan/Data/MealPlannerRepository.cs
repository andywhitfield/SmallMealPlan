using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public class MealPlannerRepository : IMealPlannerRepository
    {
        private readonly SqliteDataContext _context;
        private readonly ILogger<MealPlannerRepository> _logger;

        public MealPlannerRepository(SqliteDataContext context, ILogger<MealPlannerRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddNewMealToPlannerAsync(UserAccount userAccount, DateTime date, string description, IEnumerable<string> ingredients, string notes)
        {
            var currentMealsOnDate = await _context.PlannerMeals
                .Where(pm => pm.User == userAccount)
                .Where(pm => pm.Date == date.Date)
                .Where(pm => !pm.DeletedDateTime.HasValue)
                .CountAsync();

            _context.PlannerMeals.Add(new PlannerMeal
            {
                Date = date.Date,
                Meal = new Meal
                {
                    Description = description,
                    Notes = notes,
                    User = userAccount,
                    Ingredients = ingredients.Select((i, idx) => new MealIngredient
                    {
                        Ingredient = new Ingredient { Description = i },
                        SortOrder = idx
                    }).ToList()
                },
                User = userAccount,
                SortOrder = currentMealsOnDate
            });
            await _context.SaveChangesAsync();
        }

        public Task<List<PlannerMeal>> GetPlannerMealsAsync(UserAccount userAccount, DateTime fromDateInclusive, DateTime toDateExclusive)
        {
            return _context.PlannerMeals
                .Include(pm => pm.Meal)
                .ThenInclude(m => m.Ingredients)
                .ThenInclude(mi => mi.Ingredient)
                .Where(pm => pm.User == userAccount)
                .Where(pm => pm.Date >= fromDateInclusive)
                .Where(pm => pm.Date < toDateExclusive)
                .Where(pm => !pm.DeletedDateTime.HasValue)
                .OrderBy(pm => pm.Date)
                .ThenBy(pm => pm.SortOrder)
                .ToListAsync();
        }

        public async Task DeleteMealFromPlannerAsync(UserAccount userAccount, int mealPlannerId)
        {
            if (userAccount == null)
                throw new ArgumentNullException(nameof(userAccount));

            var plannerMeal = await _context.PlannerMeals.FindAsync(mealPlannerId);
            if (plannerMeal == null)
                return;
            _logger.LogDebug($"Deleting planner meal id: {plannerMeal.PlannerMealId}");
            plannerMeal.DeletedDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}