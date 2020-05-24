using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.Web.Model.Request;
using SmallMealPlan.Web.Model.ShoppingList;

namespace SmallMealPlan.Web.Controllers
{
    public class ShoppingListController : Controller
    {
        private readonly ILogger<ShoppingListController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IShoppingListRepository _shoppingListRepository;

        public ShoppingListController(ILogger<ShoppingListController> logger,
            IUserAccountRepository userAccountRepository,
            IShoppingListRepository shoppingListRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _shoppingListRepository = shoppingListRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            return View(new IndexViewModel(HttpContext)
            {
                MyList = (await _shoppingListRepository.GetActiveItemsAsync(user)).Select(i => new ShoppingListItemModel
                {
                    ShoppingListItemId = i.ShoppingListItemId,
                    Description = i.Ingredient.Description
                })
            });
        }

        [Authorize]
        [HttpPost("~/shoppinglist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem([FromForm] AddItemToShoppingListRequest addModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            await _shoppingListRepository.AddAsync(user, addModel.Description);
            return Redirect("~/shoppinglist");
        }

        [Authorize]
        [HttpPost("~/shoppinglist/bought/{shoppingListItemId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsBought([FromRoute] int shoppingListItemId)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var shoppingListItem = await _shoppingListRepository.GetAsync(shoppingListItemId);
            if (shoppingListItem.User != user)
                return BadRequest();
            await _shoppingListRepository.MarkAsBoughtAsync(user, shoppingListItem);
            return Redirect("~/shoppinglist");
        }
    }
}