using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using SmallMealPlan.Web.Model.Home;

namespace SmallMealPlan.Web.Model.Meals
{
    public class IndexViewModel : BaseViewModel
    {
        public const string SortByName = "name";
        public const string SortByRecentlyUsed = "recent";

        public IndexViewModel(HttpContext context) : base(context)
        {
            SelectedArea = SmpArea.Meals;
        }
        public int PageNumber { get; set; }
        public int PageCount { get; set; }
        public string Sort { get; set; }
        public bool SortedByName => Sort == SortByName;
        public bool SortedByRecentlyUsed => !SortedByName;
        public List<PlannerDayMealViewModel> Meals { get; set; } = new List<PlannerDayMealViewModel>();
    }
}