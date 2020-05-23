using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using SmallMealPlan.Web.Model.Home;

namespace SmallMealPlan.Web.Model.Meals
{
    public class IndexViewModel : BaseViewModel
    {
        public IndexViewModel(HttpContext context) : base(context)
        {
            SelectedArea = SmpArea.Meals;
        }
        public Pagination Pagination { get; set; }
        public List<PlannerDayMealViewModel> Meals { get; set; } = new List<PlannerDayMealViewModel>();
    }
}