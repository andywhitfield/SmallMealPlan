using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.Model;
using SmallMealPlan.RememberTheMilk;
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
        private readonly IRtmClient _rtmClient;

        public ShoppingListController(ILogger<ShoppingListController> logger,
            IUserAccountRepository userAccountRepository,
            IShoppingListRepository shoppingListRepository,
            IRtmClient rtmClient)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _shoppingListRepository = shoppingListRepository;
            _rtmClient = rtmClient;
        }

        public async Task<IActionResult> Index([FromQuery] int? boughtItemsPageNumber)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var (boughtItems, boughtItemsPage, boughtItemsPageCount) = await _shoppingListRepository.GetBoughtItemsAsync(user, boughtItemsPageNumber ?? 1);
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
                BoughtList = boughtItems.Select(i => new ShoppingListItemModel
                {
                    ShoppingListItemId = i.ShoppingListItemId,
                    Description = i.Ingredient.Description
                }),
                BoughtListPagination = new Pagination(boughtItemsPage, boughtItemsPageCount, Pagination.SortByRecentlyUsed, ""),
                HasRtmToken = !string.IsNullOrEmpty(user.RememberTheMilkToken)
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

        [HttpPost("~/shoppinglist/rtm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RememberTheMilk([FromForm] SyncWithRememberTheMilkRequest requestModel)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            _logger.LogTrace($"Sync with RTM: import={requestModel.Import}; export={requestModel.Export}; list={requestModel.List}");

            if (requestModel.Export ?? false)
                await ExportToRtmAsync(user, requestModel.List);

            return Redirect("~/shoppinglist");
        }

        private async Task ExportToRtmAsync(UserAccount user, string listId)
        {
            var itemsToExport = (await _shoppingListRepository.GetActiveItemsAsync(user)).Select(x => x.Ingredient.Description);

            var listTasks = await _rtmClient.GetTaskListsAsync(user.RememberTheMilkToken, listId);
            if (listTasks.List == null)
            {
                _logger.LogInformation($"HERE: listTasks.List is null");
                return;
            }

            var timelineTask = new Lazy<Task<string>>(() => _rtmClient.CreateTimelineAsync(user.RememberTheMilkToken));
            var existingItemsInList = listTasks.List.SelectMany(x => x.TaskSeries).Select(x => x.Name);
            foreach (var itemToAddToList in itemsToExport.Except(existingItemsInList))
            {
                var timeline = await timelineTask.Value;
                _logger.LogTrace($"Adding item [{itemToAddToList}] to shopping list [{listId}] using timeline [{timeline}]");
                await _rtmClient.AddTaskAsync(user.RememberTheMilkToken, timeline, listId, itemToAddToList);
            }
         }
    }
}