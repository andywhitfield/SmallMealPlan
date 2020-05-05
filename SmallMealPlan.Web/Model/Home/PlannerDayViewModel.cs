using System;
using System.Collections.Generic;
using System.Linq;

namespace SmallMealPlan.Web.Model.Home
{
    public class PlannerDayViewModel
    {
        public DateTime Day { get; set; }
        public IEnumerable<PlannerDayMealViewModel> Meals { get; set; } = Enumerable.Empty<PlannerDayMealViewModel>();
    }
}