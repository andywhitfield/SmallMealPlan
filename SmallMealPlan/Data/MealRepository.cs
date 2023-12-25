using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public class MealRepository(SqliteDataContext context,
    IDirectDbService directDbService)
    : IMealRepository
{
    private const int PageSize = 10;

    public async Task<Meal> GetAsync(int mealId)
    {
        var meal = await context.Meals.Include(m => m.Ingredients).ThenInclude(mi => mi.Ingredient).Where(m => m.MealId == mealId).SingleOrDefaultAsync();
        return meal ?? throw new ArgumentException($"MealId {mealId} not found", nameof(mealId));
    }

    public async Task<(List<Meal> Meals, int PageNumber, int PageCount)> GetMealsByMostRecentlyUsedAsync(UserAccount user, int pageNumber, string? filter)
    {
        var mealIds = await directDbService.GetMealIdsByMostRecentlyUsedAsync(user, pageNumber, PageSize, filter);
        return ((
            await context
                .Meals
                .Include(m => m.Ingredients)
                .ThenInclude(mi => mi.Ingredient)
                .Where(m => m.User == user)
                .Where(m => mealIds.MealIds.Contains(m.MealId))
                .ToListAsync())
            .OrderBy(m => mealIds.MealIds.IndexOf(m.MealId))
            .ToList(), mealIds.PageNumber, mealIds.PageCount);
    }

    public async Task<(List<Meal> Meals, int PageNumber, int PageCount)> GetMealsByNameAsync(UserAccount user, int pageNumber, string? filter)
    {
        var mealIds = await directDbService.GetMealIdsByNameAsync(user, pageNumber, PageSize, filter);
        return ((
            await context
                .Meals
                .Include(m => m.Ingredients)
                .ThenInclude(mi => mi.Ingredient)
                .Where(m => m.User == user)
                .Where(m => mealIds.MealIds.Contains(m.MealId))
                .ToListAsync())
            .OrderBy(m => mealIds.MealIds.IndexOf(m.MealId))
            .ToList(), mealIds.PageNumber, mealIds.PageCount);
    }

    public async Task AddNewMealAsync(UserAccount user, string description, IEnumerable<string> ingredients, string? notes)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        context.Meals.Add(new Meal
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
        await context.SaveChangesAsync();
    }

    public async Task UpdateMealAsync(UserAccount user, Meal meal, string description, IEnumerable<string> ingredients, string? notes)
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
                context.MealIngredients.RemoveRange(meal.Ingredients);
            if (ingredients.Any())
            {
                meal.Ingredients = ingredients.Select((i, idx) => new MealIngredient
                {
                    Ingredient = new Ingredient { Description = i, CreatedBy = user },
                    SortOrder = idx
                }).ToList();
            }
        }
        await context.SaveChangesAsync();
    }

    public async Task DeleteMealAsync(UserAccount user, Meal meal)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        if (meal == null)
            throw new ArgumentNullException(nameof(meal));

        if (meal.User.UserAccountId != user.UserAccountId)
            throw new SecurityException($"Cannot delete meal id: {meal.MealId}");

        await directDbService.RemovePlannerMealByMealIdAsync(meal.MealId);
        context.Meals.Remove(meal);
        await context.SaveChangesAsync();
    }
}