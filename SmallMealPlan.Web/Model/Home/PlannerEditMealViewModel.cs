using System;
using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.Home
{
    public class PlannerEditMealViewModel : BaseViewModel
    {
        public PlannerEditMealViewModel(HttpContext context, DateTime day) : base(context)
        {
            Day = day;
            SelectedArea = SmpArea.Planner;
        }

        public DateTime Day { get; }
    }
}