using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.RememberTheMilk;

namespace SmallMealPlan.Web.Controllers
{
    [Authorize]
    public class RtmController : Controller
    {
        private readonly ILogger<RtmController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IRtmClient _rtmClient;
        private readonly RtmConfig _rtmConfig;

        public RtmController(ILogger<RtmController> logger,
            IUserAccountRepository userAccountRepository,
            IRtmClient rtmClient,
            RtmConfig rtmConfig)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _rtmClient = rtmClient;
            _rtmConfig = rtmConfig;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string frob)
        {
            if (string.IsNullOrEmpty(frob))
                return BadRequest();

            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var tokenResponse = await _rtmClient.GetTokenAsync(frob);
            // TODO: update user with the RTM token
            _logger.LogInformation($"Updating user {user.UserAccountId} with token: {tokenResponse.Token}");
            return Redirect("~/shoppinglist");
        }

        [HttpGet("~/rtm/link")]
        public IActionResult Link()
        {
            var rtmAuthUri = RtmAuthenticationHelper.BuildAuthenticationUri(_rtmConfig, RtmPermission.Write);
            _logger.LogTrace($"Redirecting to RTM: {rtmAuthUri}");
            return Redirect(rtmAuthUri.ToString());
        }
    }
}