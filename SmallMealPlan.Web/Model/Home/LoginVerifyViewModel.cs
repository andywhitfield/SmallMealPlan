using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.Home;

public class LoginVerifyViewModel(HttpContext context, string? returnUrl, string email, bool isReturningUser, string verifyOptions)
    : BaseViewModel(context)
{
    public string? ReturnUrl { get; } = returnUrl;
    public string Email { get; } = email;
    public bool IsReturningUser { get; } = isReturningUser;
    public string VerifyOptions { get; } = verifyOptions;
}