using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.Home
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel(HttpContext context) : base(context) { }
    }
}