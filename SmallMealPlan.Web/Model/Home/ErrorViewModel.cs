using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.Home
{
    public class ErrorViewModel : BaseViewModel
    {
        public ErrorViewModel(HttpContext context) : base(context) { }
    }
}