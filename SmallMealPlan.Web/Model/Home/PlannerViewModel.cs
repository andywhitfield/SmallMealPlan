using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.Home
{
    public class PlannerViewModel : BaseViewModel
    {
        public const string SortByName = "name";
        public const string SortByRecentlyUsed = "recent";

        public PlannerViewModel(HttpContext context, DateTime day) : base(context)
        {
            Day = day;
            SelectedArea = SmpArea.Planner;
        }

        public DateTime Day { get; }
        public int PageNumber { get; set; }
        public int PageCount { get; set; }
        public string Sort { get; set; }
        public bool SortedByName => Sort == SortByName;
        public bool SortedByRecentlyUsed => !SortedByName;
        public List<PlannerDayMealViewModel> Meals { get; set; } = new List<PlannerDayMealViewModel>();
    }
}