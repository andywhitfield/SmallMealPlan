using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.Home;

public class PlannerViewModel : BaseViewModel
{
    public PlannerViewModel(HttpContext context, DateTime day) : base(context)
    {
        Day = day;
        SelectedArea = SmpArea.Planner;
    }

    public DateTime Day { get; }
    public Pagination? Pagination { get; set; }
    public List<PlannerDayMealViewModel> Meals { get; set; } = new List<PlannerDayMealViewModel>();
}