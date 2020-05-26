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
    public class ShoppingListApiController : ControllerBase
    {
        private readonly ILogger<ShoppingListApiController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IShoppingListRepository _shoppingListRepository;

        public ShoppingListApiController(ILogger<ShoppingListApiController> logger,
            IUserAccountRepository userAccountRepository,
            IShoppingListRepository shoppingListRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _shoppingListRepository = shoppingListRepository;
        }

        [HttpPut("~/api/shoppinglist/{shoppingListItemId}/move")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Move(int shoppingListItemId, ShoppingListItemMoveRequest shoppingListItemMoveRequest)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            _logger.LogTrace($"Moving shopping list item {shoppingListItemId} to after {shoppingListItemMoveRequest.SortOrderPreviousShoppingListItemId}");
            await _shoppingListRepository.ReorderAsync(user, shoppingListItemId, shoppingListItemMoveRequest.SortOrderPreviousShoppingListItemId);
            return NoContent();
        }
    }
}