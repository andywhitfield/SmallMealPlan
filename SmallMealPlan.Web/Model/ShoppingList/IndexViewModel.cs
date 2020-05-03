using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.ShoppingList
{
    public class IndexViewModel : BaseViewModel
    {
        public IndexViewModel(HttpContext context) : base(context)
        {
            SelectedArea = SmpArea.ShoppingList;
        }
    }
}