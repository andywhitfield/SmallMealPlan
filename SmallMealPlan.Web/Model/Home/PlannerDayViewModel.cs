using System;
using System.Collections.Generic;

namespace SmallMealPlan.Web.Model.Home
{
    public class PlannerDayViewModel
    {
        public DateTime Day { get; set; }
        public List<PlannerDayMealViewModel> Meals { get; set; } = new List<PlannerDayMealViewModel>();
    }
}