using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.SmallLister;

namespace SmallMealPlan.Web.Controllers
{
    [Authorize]
    public class SmallListerController : Controller
    {
        private readonly ILogger<SmallListerController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly SmallListerConfig _smlConfig;

        public SmallListerController(ILogger<SmallListerController> logger,
            IUserAccountRepository userAccountRepository,
            SmallListerConfig smlConfig)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _smlConfig = smlConfig;
        }

        [HttpGet("~/sml")]
        public async Task<IActionResult> Index([FromQuery, Required] string refreshToken)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userAccountRepository.GetUserAccountAsync(User);
            await _userAccountRepository.UpdateSmallListerTokenAsync(user, refreshToken);
            _logger.LogInformation($"Updating user {user.UserAccountId} with token: {refreshToken}");
            return Redirect("~/shoppinglist");
        }

        [HttpGet("~/sml/link")]
        public IActionResult Link()
        {
            var callbackUri = $"{Request.Scheme}://{Request.Host}/sml";
            var smlUri = $"{_smlConfig.BaseUri}api/v1/authorize?appkey={_smlConfig.AppKey}&redirect_uri={HttpUtility.UrlEncode(callbackUri)}";
            return Redirect(smlUri);
        }

        [HttpPost("~/sml/unlink")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlink()
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            await _userAccountRepository.UpdateSmallListerTokenAsync(user, null);
            _logger.LogInformation($"Clearing SmallLister token from user {user.UserAccountId}");
            return Redirect("~/shoppinglist");
        }
    }
}