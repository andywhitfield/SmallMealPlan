using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.Web.Model.Request;

namespace SmallMealPlan.Web.Controllers
{
    [ApiController]
    [Authorize]
    public class HomeApiController : ControllerBase
    {
        private readonly ILogger<HomeApiController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IPlannerMealRepository _plannerMealRepository;

        public HomeApiController(ILogger<HomeApiController> logger,
            IUserAccountRepository userAccountRepository,
            IPlannerMealRepository plannerMealRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _plannerMealRepository = plannerMealRepository;
        }

        [HttpPut("~/planner/{plannerMealId}/move")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Move(int plannerMealId, PlannerMealMoveRequest plannerMealMoveRequest)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            await _plannerMealRepository.UpdateAsync(user, plannerMealId, plannerMealMoveRequest.Date.ParseDateOrToday(), plannerMealMoveRequest.SortOrderPreviousPlannerMealId);
            return NoContent();
        }
    }
}