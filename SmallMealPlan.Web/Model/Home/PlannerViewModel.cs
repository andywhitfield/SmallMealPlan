using System;
using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.Home
{
    public class PlannerViewModel : BaseViewModel
    {
        public PlannerViewModel(HttpContext context, DateTime date) : base(context)
        {
            Date = date;
            SelectedArea = SmpArea.Planner;
        }

        public DateTime Date { get; }
    }
}