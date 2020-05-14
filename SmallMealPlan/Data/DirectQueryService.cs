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
    public class DirectQueryService : IDirectQueryService
    {
        private readonly SqliteDataContext _context;
        private readonly ILogger<DirectQueryService> _logger;

        public DirectQueryService(SqliteDataContext context, ILogger<DirectQueryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(List<int> MealIds, int PageNumber, int PageCount)> GetMealIdsByMostRecentlyUsedAsync(UserAccount user, int pageNumber, int pageSize)
        {
            var meals = await GetAsync(user);
            var totalCount = meals.Count;
            var pageCount = totalCount / pageSize;
            if (totalCount % pageSize != 0) pageCount++;
            var pageIndex = Math.Min(pageCount, Math.Max(0, pageNumber - 1));

            var mealIds = meals
                .OrderByDescending(m => m.DateOnPlanner ?? DateTime.MinValue)
                .ThenByDescending(m => m.MealCreatedDate)
                .Select(m => m.MealId)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList();

            return (mealIds, pageIndex + 1, pageCount);
        }

        public async Task<(List<int> MealIds, int PageNumber, int PageCount)> GetMealIdsByNameAsync(UserAccount user, int pageNumber, int pageSize)
        {
            return (new List<int>(), 1, 1);
        }

        private async Task<List<(int MealId, DateTime MealCreatedDate, DateTime? DateOnPlanner)>> GetAsync(UserAccount user)
        {
            var mealIds = new List<(int, DateTime, DateTime?)>();
            foreach (var mealInfo in await _context.Database.GetDbConnection().QueryAsync(
                @"select m.MealId, m.CreatedDateTime, pm.Date
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
                mealIds.Add(((int)mealInfo.MealId, ToDateTime(mealInfo.CreatedDateTime) ?? DateTime.MinValue, ToDateTime(mealInfo.Date)?.Date));

            return mealIds;

            DateTime? ToDateTime(object val) =>
                val is string s
                ? (DateTime.TryParseExact(s, new[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.ffffff" }, null, DateTimeStyles.AssumeUniversal, out var d) ? (DateTime?)d : null)
                : null;

        }
    }
}