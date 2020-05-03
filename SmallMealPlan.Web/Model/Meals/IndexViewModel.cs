using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.Meals
{
    public class IndexViewModel : BaseViewModel
    {
        public IndexViewModel(HttpContext context) : base(context)
        {
            SelectedArea = SmpArea.Meals;
        }
    }
}