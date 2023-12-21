using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.RememberTheMilk;

namespace SmallMealPlan.Web.Controllers;

[Authorize]
public class RtmController(ILogger<RtmController> logger,
    IUserAccountRepository userAccountRepository,
    IRtmClient rtmClient,
    RtmConfig rtmConfig) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string? frob)
    {
        if (string.IsNullOrEmpty(frob))
            return BadRequest();

        var user = await userAccountRepository.GetUserAccountAsync(User);
        var tokenResponse = await rtmClient.GetTokenAsync(frob);
        user.RememberTheMilkToken = tokenResponse.Token;
        await userAccountRepository.UpdateAsync(user);
        logger.LogInformation($"Updating user {user.UserAccountId} with token: {tokenResponse.Token}");
        return Redirect("~/shoppinglist");
    }

    [HttpGet("~/rtm/link")]
    public IActionResult Link()
    {
        var rtmAuthUri = RtmAuthenticationHelper.BuildAuthenticationUri(rtmConfig, RtmPermission.Write);
        logger.LogTrace($"Redirecting to RTM: {rtmAuthUri}");
        return Redirect(rtmAuthUri.ToString());
    }

    [HttpPost("~/rtm/unlink")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlink()
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        user.RememberTheMilkToken = null;
        user.RememberTheMilkLastListId = null;
        await userAccountRepository.UpdateAsync(user);
        logger.LogInformation($"Clearing RTM token from user {user.UserAccountId}");
        return Redirect("~/shoppinglist");
    }
}