using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public class DirectDbService(SqliteDataContext context) : IDirectDbService
{
    public async Task<(List<int> MealIds, int PageNumber, int PageCount)> GetMealIdsByMostRecentlyUsedAsync(UserAccount user, int pageNumber, int pageSize, string? filter)
    {
        var meals = await GetAsync(user, filter);
        var pagination = Paging.GetPageInfo(meals.Count, pageSize, pageNumber);

        var mealIds = meals
            .OrderByDescending(m => m.DateOnPlanner ?? DateTime.MinValue)
            .ThenByDescending(m => m.MealCreatedDate)
            .Select(m => m.MealId)
            .Skip(pagination.PageIndex * pageSize)
            .Take(pageSize)
            .ToList();

        return (mealIds, pagination.PageIndex + 1, pagination.PageCount);
    }

    public async Task<(List<int> MealIds, int PageNumber, int PageCount)> GetMealIdsByNameAsync(UserAccount user, int pageNumber, int pageSize, string? filter)
    {
        var meals = await GetAsync(user, filter);
        var pagination = Paging.GetPageInfo(meals.Count, pageSize, pageNumber);

        var mealIds = meals
            .OrderBy(m => m.MealDescription)
            .ThenByDescending(m => m.DateOnPlanner ?? DateTime.MinValue)
            .ThenByDescending(m => m.MealCreatedDate)
            .Select(m => m.MealId)
            .Skip(pagination.PageIndex * pageSize)
            .Take(pageSize)
            .ToList();

        return (mealIds, pagination.PageIndex + 1, pagination.PageCount);
    }

    public Task RemovePlannerMealByMealIdAsync(int mealId) => context.Database.GetDbConnection().ExecuteAsync(
            "delete from PlannerMeals where MealId = @mealId", new { mealId });

    private async Task<List<(int MealId, DateTime MealCreatedDate, DateTime? DateOnPlanner, string MealDescription)>> GetAsync(UserAccount user, string? filter)
    {
        var mealIds = new List<(int, DateTime, DateTime?, string)>();
        var hasTextFilter = !string.IsNullOrWhiteSpace(filter);
        var mealQuery = new StringBuilder();
        mealQuery.AppendLine(
            @"select distinct m.MealId, m.CreatedDateTime, pm.Date, m.Description
                from Meals m
                left join (
                    select max(Date) Date, MealId
                    from PlannerMeals
                    where UserAccountId = @UserAccountId
                    and DeletedDateTime is null
                    group by MealId
                ) pm
                on m.MealId = pm.MealId");

        if (hasTextFilter)
        {
            mealQuery.AppendLine(
                @"left join MealIngredients mi on m.MealId = mi.MealId
                    left join Ingredients i on mi.IngredientId = i.IngredientId");
        }

        mealQuery.AppendLine(
            @"where m.UserAccountId = @UserAccountId
                and m.DeletedDateTime is null");

        if (hasTextFilter)
        {
            mealQuery.AppendLine(
                @"and (m.Description like @Filter
                    or m.Notes like @Filter
                    or i.Description like @Filter)");
        }

        foreach (var mealInfo in await context.Database.GetDbConnection().QueryAsync(mealQuery.ToString(), new { user.UserAccountId, Filter = $"%{filter?.Trim()}%" }))
            mealIds.Add(((int)mealInfo.MealId, ToDateTime(mealInfo.CreatedDateTime) ?? DateTime.MinValue, ToDateTime(mealInfo.Date)?.Date, (string)mealInfo.Description));

        return mealIds;

        DateTime? ToDateTime(object val) =>
            val is string s
            ? (DateTime.TryParseExact(s, new[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.ffffff" }, null, DateTimeStyles.AssumeUniversal, out var d) ? (DateTime?)d : null)
            : null;
    }
}