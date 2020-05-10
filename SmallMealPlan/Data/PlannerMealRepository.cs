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
    public class PlannerMealRepository : IPlannerMealRepository
    {
        private readonly SqliteDataContext _context;
        private readonly ILogger<PlannerMealRepository> _logger;

        public PlannerMealRepository(SqliteDataContext context, ILogger<PlannerMealRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddNewMealToPlannerAsync(UserAccount user, DateTime date, string description, IEnumerable<string> ingredients, string notes)
        {
            var currentMealsOnDate = await _context.PlannerMeals
                .Where(pm => pm.User == user)
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
                    User = user,
                    Ingredients = ingredients.Select((i, idx) => new MealIngredient
                    {
                        Ingredient = new Ingredient { Description = i, CreatedBy = user },
                        SortOrder = idx
                    }).ToList()
                },
                User = user,
                SortOrder = currentMealsOnDate
            });
            await _context.SaveChangesAsync();
        }

        public Task<List<PlannerMeal>> GetPlannerMealsAsync(UserAccount user, DateTime fromDateInclusive, DateTime toDateExclusive)
        {
            return _context.PlannerMeals
                .Include(pm => pm.Meal)
                .ThenInclude(m => m.Ingredients)
                .ThenInclude(mi => mi.Ingredient)
                .Where(pm => pm.User == user)
                .Where(pm => pm.Date >= fromDateInclusive)
                .Where(pm => pm.Date < toDateExclusive)
                .Where(pm => !pm.DeletedDateTime.HasValue)
                .OrderBy(pm => pm.Date)
                .ThenBy(pm => pm.SortOrder)
                .ToListAsync();
        }

        public async Task DeleteMealFromPlannerAsync(UserAccount user, int mealPlannerId)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var plannerMeal = await _context.PlannerMeals.FindAsync(mealPlannerId);
            if (plannerMeal == null)
                return;

            if (plannerMeal.User.UserAccountId != user.UserAccountId)
                throw new SecurityException($"Cannot delete planner meal id: {plannerMeal.PlannerMealId}");

            _logger.LogDebug($"Deleting planner meal id: {plannerMeal.PlannerMealId}");
            plannerMeal.DeletedDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserAccount user, int mealPlannerId, DateTime date, int? sortOrderPreviousPlannerMealId)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var plannerMeal = await _context.PlannerMeals.FindAsync(mealPlannerId);
            if (plannerMeal == null)
                return;

            if (plannerMeal.User.UserAccountId != user.UserAccountId)
                throw new SecurityException($"Cannot update planner meal id: {plannerMeal.PlannerMealId}");

            _logger.LogDebug($"Update planner meal id: {plannerMeal.PlannerMealId} to be on date {date} and after {sortOrderPreviousPlannerMealId}");

            var mealsOnDate = _context.PlannerMeals
                .Where(pm => pm.User == user)
                .Where(pm => pm.Date == date.Date)
                .Where(pm => !pm.DeletedDateTime.HasValue)
                .OrderBy(pm => pm.SortOrder)
                .AsAsyncEnumerable();
            
            int? sortOrder = null;
            PlannerMeal lastMealOnDate = null;
            if (!sortOrderPreviousPlannerMealId.HasValue)
            {
                sortOrder = 0;
                plannerMeal.SortOrder = sortOrder.Value;
                sortOrder++;
            }

            await foreach (var mealOnDate in mealsOnDate)
            {
                if (mealOnDate.PlannerMealId == plannerMeal.PlannerMealId)
                    continue;

                if (sortOrderPreviousPlannerMealId == mealOnDate.PlannerMealId)
                {
                    sortOrder = mealOnDate.SortOrder + 1;
                    plannerMeal.SortOrder = sortOrder.Value;
                    sortOrder++;
                }
                else if (sortOrder.HasValue)
                {
                    mealOnDate.SortOrder = sortOrder.Value;
                    sortOrder++;
                }

                lastMealOnDate = mealOnDate;
            }

            plannerMeal.Date = date.Date;
            plannerMeal.LastUpdateDateTime = DateTime.UtcNow;
            if (!sortOrder.HasValue)
                plannerMeal.SortOrder = lastMealOnDate?.SortOrder ?? 0;

            await _context.SaveChangesAsync();
        }
    }
}