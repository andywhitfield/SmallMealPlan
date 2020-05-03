using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.Home
{
    public class PlannerViewModel : BaseViewModel
    {
        public PlannerViewModel(HttpContext context) : base(context)
        {
            SelectedArea = SmpArea.Planner;
        }
    }
}