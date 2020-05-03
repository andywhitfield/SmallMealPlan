using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.Home
{
    public class IndexViewModel : BaseViewModel
    {
        public IndexViewModel(HttpContext context) : base(context)
        {
            SelectedArea = SmpArea.Planner;
        }
    }
}