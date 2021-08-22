using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.RememberTheMilk;
using SmallMealPlan.Web.Model.ShoppingList;

namespace SmallMealPlan.Web.Controllers
{
    [ApiController]
    [Authorize]
    public class RtmApiController : ControllerBase
    {
        private readonly ILogger<RtmApiController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IRtmClient _rtmClient;

        public RtmApiController(ILogger<RtmApiController> logger,
            IUserAccountRepository userAccountRepository,
            IRtmClient rtmClient)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _rtmClient = rtmClient;
        }

        [HttpGet("~/api/rtm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetListsResponse>> GetLists()
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            if (string.IsNullOrEmpty(user.RememberTheMilkToken))
                return BadRequest();

            var rtmList = await _rtmClient.GetListsAsync(user.RememberTheMilkToken);
            if (rtmList.List == null)
                return new GetListsResponse();

            return new GetListsResponse { Options = rtmList.List.Select(l => new GetListsResponse.ListItem(l.Id, l.Name, !string.IsNullOrEmpty(user.RememberTheMilkLastListId) && user.RememberTheMilkLastListId == l.Id)).ToArray() };
        }
    }
}