using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public class DirectDbService : IDirectDbService
    {
        private readonly SqliteDataContext _context;
        private readonly ILogger<DirectDbService> _logger;

        public DirectDbService(SqliteDataContext context, ILogger<DirectDbService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(List<int> MealIds, int PageNumber, int PageCount)> GetMealIdsByMostRecentlyUsedAsync(UserAccount user, int pageNumber, int pageSize)
        {
            var meals = await GetAsync(user);
            var pagination = GetPagination(meals.Count, pageSize, pageNumber);

            var mealIds = meals
                .OrderByDescending(m => m.DateOnPlanner ?? DateTime.MinValue)
                .ThenByDescending(m => m.MealCreatedDate)
                .Select(m => m.MealId)
                .Skip(pagination.PageIndex * pageSize)
                .Take(pageSize)
                .ToList();

            return (mealIds, pagination.PageIndex + 1, pagination.PageCount);
        }

        public async Task<(List<int> MealIds, int PageNumber, int PageCount)> GetMealIdsByNameAsync(UserAccount user, int pageNumber, int pageSize)
        {
            var meals = await GetAsync(user);
            var pagination = GetPagination(meals.Count, pageSize, pageNumber);

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

        public Task RemovePlannerMealByMealIdAsync(int mealId) => _context.Database.GetDbConnection().ExecuteAsync(
                "delete from PlannerMeals where MealId = @mealId", new { mealId });

        private (int PageIndex, int PageCount) GetPagination(int totalCount, int pageSize, int pageNumber)
        {
            var pageCount = totalCount / pageSize;
            if (totalCount % pageSize != 0) pageCount++;
            var pageIndex = Math.Min(pageCount - 1, Math.Max(0, pageNumber - 1));
            return (pageIndex, pageCount);
        }

        private async Task<List<(int MealId, DateTime MealCreatedDate, DateTime? DateOnPlanner, string MealDescription)>> GetAsync(UserAccount user)
        {
            var mealIds = new List<(int, DateTime, DateTime?, string)>();
            foreach (var mealInfo in await _context.Database.GetDbConnection().QueryAsync(
                @"select m.MealId, m.CreatedDateTime, pm.Date, m.Description
                from Meals m
                left join (
                    select max(Date) Date, MealId
                    from PlannerMeals
                    where UserAccountId = @UserAccountId
                    and DeletedDateTime is null
                    group by MealId
                ) pm
                on m.MealId = pm.MealId
                where m.UserAccountId = @UserAccountId
                and m.DeletedDateTime is null", new { user.UserAccountId }))
                mealIds.Add(((int)mealInfo.MealId, ToDateTime(mealInfo.CreatedDateTime) ?? DateTime.MinValue, ToDateTime(mealInfo.Date)?.Date, (string)mealInfo.Description));

            return mealIds;

            DateTime? ToDateTime(object val) =>
                val is string s
                ? (DateTime.TryParseExact(s, new[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.ffffff" }, null, DateTimeStyles.AssumeUniversal, out var d) ? (DateTime?)d : null)
                : null;

        }
    }
}