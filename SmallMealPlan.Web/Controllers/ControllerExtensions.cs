using System;
using System.Globalization;

namespace SmallMealPlan.Web.Controllers
{
    public static class ControllerExtensions
    {
        public static DateTime ParseDateOrToday(this string date) => DateTime.TryParseExact(date, "yyyyMMdd", null, DateTimeStyles.AssumeUniversal, out var dt) ? dt : DateTime.Today;
    }
}