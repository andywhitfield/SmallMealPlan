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
    [Authorize]
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

        public async Task<IActionResult> Index()
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            return View(new IndexViewModel(HttpContext)
            {
                MyList = (await _shoppingListRepository.GetActiveItemsAsync(user)).Select(i => new ShoppingListItemModel
                {
                    ShoppingListItemId = i.ShoppingListItemId,
                    Description = i.Ingredient.Description
                }),
                IngredientFromPlannerList = (await _shoppingListRepository.GetUnboughtIngredientsFromPlannerAsync(user)).Select(i => new IngredientItemModel
                {
                    IngredientId = i.IngredientId,
                    Description = i.Description
                }),
                BoughtList = (await _shoppingListRepository.GetBoughtItemsAsync(user, 1)).Select(i => new ShoppingListItemModel
                {
                    ShoppingListItemId = i.ShoppingListItemId,
                    Description = i.Ingredient.Description
                })
            });
        }

        [HttpPost("~/shoppinglist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem([FromForm] AddItemToShoppingListRequest addModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            await _shoppingListRepository.AddNewIngredientAsync(user, addModel.Description);
            return Redirect("~/shoppinglist");
        }

        [HttpPost("~/shoppinglist/add/planner")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIngredientsFromPlanner([FromForm] AddItemToShoppingListFromPlannerRequest addModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userAccountRepository.GetUserAccountAsync(User);
            _logger.LogTrace($"Adding ingredients [{string.Join(',', addModel.IngredientId)}] from planner");
            await _shoppingListRepository.AddIngredientsAsync(user, addModel.IngredientId);
            return Redirect("~/shoppinglist");
        }

        [HttpGet("~/shoppinglist/add/planner/{ingredientId}")]
        public async Task<IActionResult> AddSingleIngredientFromPlanner([FromRoute] int ingredientId)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            _logger.LogTrace($"Adding ingredient {ingredientId} from planner");
            await _shoppingListRepository.AddIngredientsAsync(user, ingredientId);
            return Redirect("~/shoppinglist");
        }
        
        [HttpGet("~/shoppinglist/add/{shoppingListItemId}")]
        public async Task<IActionResult> MakeAsNotBought(int shoppingListItemId)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var shoppingListItem = await _shoppingListRepository.GetAsync(shoppingListItemId);
            if (shoppingListItem.User != user)
                return BadRequest();
            if (shoppingListItem.BoughtDateTime != null)
                await _shoppingListRepository.MarkAsActiveAsync(user, shoppingListItem);
            return Redirect("~/shoppinglist");
        }

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