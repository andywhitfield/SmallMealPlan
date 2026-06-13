using SmallMealPlan.Data;
using SmallMealPlan.Web.Model;

namespace SmallMealPlan.Web;

public class UserAccountCurrentAreaMiddleware(
    ILogger<UserAccountCurrentAreaMiddleware> logger,
    IUserAccountRepository userAccountRepository)
    : IMiddleware
{
    private static readonly CookieOptions _redirCookieOptions = new() { HttpOnly = true, IsEssential = false, Path = "/", Secure = true };
    private static readonly IEnumerable<string> _knownAreaPaths = [.. Enum.GetNames<SmpArea>().Select(a => "/" + a.ToLowerInvariant())];
    private static readonly string _plannerArea = "/" + SmpArea.Planner.ToString().ToLowerInvariant();

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.Value == "/signout")
        {
            context.Response.Cookies.Delete("redir-user-area", _redirCookieOptions);
        }
        else if (context.User?.Identity?.IsAuthenticated ?? false)
        {
            try
            {
                if (context.Request.Cookies["redir-user-area"] == null)
                {
                    logger.LogTrace("User is authenticated, but has no 'current user area' cookie set");
                    context.Response.Cookies.Append("redir-user-area", "true", _redirCookieOptions);
                    if (context.Request.Path.Value == "/")
                    {
                        logger.LogTrace("Request is for the root page, checking the user's last active area");
                        var user = await userAccountRepository.GetUserAccountAsync(context.User);
                        if (user.CurrentArea != null)
                        {
                            logger.LogTrace("User has current area of {CurrentArea}, redirecting", user.CurrentArea);
                            context.Response.Redirect(user.CurrentArea);
                            return;
                        }

                        logger.LogTrace("Updating user's current area setting to planner");
                        user.CurrentArea = _plannerArea;
                        await userAccountRepository.UpdateAsync(user);
                    }
                }
                else
                {
                    var area = GetArea(context.Request.Path.Value ?? "");
                    if (area != null)
                    {
                        logger.LogTrace("Checking if the user's current area setting needs updating");
                        var user = await userAccountRepository.GetUserAccountAsync(context.User);
                        if (user.CurrentArea != area)
                        {
                            logger.LogTrace("Updating user's current area setting to {Area}", area);
                            user.CurrentArea = area;
                            await userAccountRepository.UpdateAsync(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not update/check for the user's current area");
            }
        }

        await next(context);
    }

    private static string? GetArea(string path)
    {
        if (path == "/")
            return null;
        path = path.ToLowerInvariant();
        foreach (var area in _knownAreaPaths)
            if (path.StartsWith(area)) return area;
        return null;
    }
}
