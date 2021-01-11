using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.Model;
using SmallMealPlan.RememberTheMilk;
using SmallMealPlan.SmallLister;
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
        private readonly IMealRepository _mealRepository;
        private readonly IRtmClient _rtmClient;
        private readonly ISmallListerClient _smlClient;

        public ShoppingListController(ILogger<ShoppingListController> logger,
            IUserAccountRepository userAccountRepository,
            IShoppingListRepository shoppingListRepository,
            IMealRepository mealRepository,
            IRtmClient rtmClient,
            ISmallListerClient smlClient)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _shoppingListRepository = shoppingListRepository;
            _mealRepository = mealRepository;
            _rtmClient = rtmClient;
            _smlClient = smlClient;
        }

        public async Task<IActionResult> Index([FromQuery] int? boughtItemsPageNumber)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var activeShoppingList = await _shoppingListRepository.GetActiveItemsAsync(user);
            var shoppingListItems = activeShoppingList.Select(mi => mi.Ingredient.Description).ToHashSet();
            var futureMealIngredients = await _shoppingListRepository.GetFutureMealIngredientsFromPlannerAsync(user);
            var (boughtItems, boughtItemsPage, boughtItemsPageCount) = await _shoppingListRepository.GetBoughtItemsAsync(user, boughtItemsPageNumber ?? 1);
            return View(new IndexViewModel(HttpContext)
            {
                MyList = activeShoppingList.Select(i => new ShoppingListItemModel
                {
                    ShoppingListItemId = i.ShoppingListItemId,
                    Description = i.Ingredient.Description
                }),
                MealFromPlannerList = futureMealIngredients
                    .Where(mi => !shoppingListItems.Contains(mi.Ingredient.Description, StringComparer.InvariantCultureIgnoreCase))
                    .GroupBy(mi => mi.Meal)
                    .Select(g => new MealItemModel
                    {
                        MealId = g.Key.MealId,
                        Description = g.Key.Description,
                        Ingredients = g.Select(i => new IngredientItemModel
                        {
                            IngredientId = i.Ingredient.IngredientId,
                            Description = i.Ingredient.Description
                        })
                    }),
                BoughtList = boughtItems.Select(i => new ShoppingListItemModel
                {
                    ShoppingListItemId = i.ShoppingListItemId,
                    Description = i.Ingredient.Description
                }),
                BoughtListPagination = new Pagination(boughtItemsPage, boughtItemsPageCount, Pagination.SortByRecentlyUsed, ""),
                HasRtmToken = !string.IsNullOrEmpty(user.RememberTheMilkToken),
                HasSmallListerToken = !string.IsNullOrEmpty(user.SmallListerToken)
            });
        }

        [HttpPost("~/shoppinglist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem([FromForm] AddItemToShoppingListRequest addModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            await _shoppingListRepository.AddNewIngredientAsync(user, addModel.Description?.Trim());
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

        [HttpGet("~/shoppinglist/add/planner/meal/{mealId}")]
        public async Task<IActionResult> AddIngredientsForMeal([FromRoute] int mealId)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            _logger.LogTrace($"Adding ingredients for meal {mealId}");
            var meal = await _mealRepository.GetAsync(mealId);
            if (meal.User != user)
                return BadRequest();

            var activeShoppingList = await _shoppingListRepository.GetActiveItemsAsync(user);
            var shoppingListItems = activeShoppingList.Select(mi => mi.Ingredient.Description).ToHashSet();

            await _shoppingListRepository.AddIngredientsAsync(
                user, meal
                    .Ingredients
                    .Where(i => !shoppingListItems.Contains(i.Ingredient.Description, StringComparer.InvariantCultureIgnoreCase))
                    .Select(li => li.Ingredient.IngredientId).ToArray());
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

        [HttpPost("~/shoppinglist/delete/{shoppingListItemId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromRoute] int shoppingListItemId)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var shoppingListItem = await _shoppingListRepository.GetAsync(shoppingListItemId);
            if (shoppingListItem.User != user)
                return BadRequest();
            await _shoppingListRepository.DeleteAsync(user, shoppingListItem);
            return Redirect("~/shoppinglist");
        }

        [HttpPost("~/shoppinglist/rtm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RememberTheMilk([FromForm] SyncRequest requestModel)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            _logger.LogTrace($"Sync with RTM: import={requestModel.Import}; export={requestModel.Export}; list={requestModel.List}");

            if (requestModel.Export ?? false)
                await ExportToRtmAsync(user, requestModel.List);
            else if (requestModel.Import ?? false)
                await ImportFromRtmAsync(user, requestModel.List);
            else
                return BadRequest();

            return Redirect("~/shoppinglist");
        }

        private async Task ExportToRtmAsync(UserAccount user, string listId)
        {
            var itemsToExport = (await _shoppingListRepository.GetActiveItemsAsync(user)).Select(x => x.Ingredient.Description);

            var listTasks = await _rtmClient.GetTaskListsAsync(user.RememberTheMilkToken, listId);
            if (listTasks.List == null)
                return;

            var timelineTask = new Lazy<Task<string>>(() => _rtmClient.CreateTimelineAsync(user.RememberTheMilkToken));
            var existingItemsInList = listTasks.List.SelectMany(x => x.TaskSeries).Select(x => x.Name);
            foreach (var itemToAddToList in itemsToExport.Except(existingItemsInList))
            {
                var timeline = await timelineTask.Value;
                _logger.LogTrace($"Adding item [{itemToAddToList}] to RTM list [{listId}] using timeline [{timeline}]");
                await _rtmClient.AddTaskAsync(user.RememberTheMilkToken, timeline, listId, itemToAddToList);
            }
        }

        private async Task ImportFromRtmAsync(UserAccount user, string listId)
        {
            var listTasks = await _rtmClient.GetTaskListsAsync(user.RememberTheMilkToken, listId);
            if (listTasks.List == null)
                return;
            var itemsToImport = listTasks.List.SelectMany(x => x.TaskSeries).Select(x => x.Name);

            var currentList = (await _shoppingListRepository.GetActiveItemsAsync(user)).Select(i => i.Ingredient.Description);
            foreach (var itemToAddToShoppingList in itemsToImport.Except(currentList, StringComparer.InvariantCultureIgnoreCase))
            {
                _logger.LogTrace($"Adding item [{itemToAddToShoppingList}] to shopping list");
                await _shoppingListRepository.AddNewIngredientAsync(user, itemToAddToShoppingList);
            }
        }

        [HttpPost("~/shoppinglist/sml")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SmallLister([FromForm] SyncRequest requestModel)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            _logger.LogTrace($"Sync with SML: import={requestModel.Import}; export={requestModel.Export}; list={requestModel.List}");

            if (requestModel.Export ?? false)
                await ExportToSmlAsync(user, requestModel.List);
            else if (requestModel.Import ?? false)
                await ImportFromSmlAsync(user, requestModel.List);
            else
                return BadRequest();

            return Redirect("~/shoppinglist");
        }

        private async Task ExportToSmlAsync(UserAccount user, string listId)
        {
            var itemsToExport = (await _shoppingListRepository.GetActiveItemsAsync(user)).Select(x => x.Ingredient.Description.Trim());
            var list = await _smlClient.GetListAsync(user.SmallListerToken, listId);
            if (list == null)
            {
                _logger.LogInformation($"could not get list {listId}");
                return;
            }

            var existingItemsInList = list.Items.Select(x => x.Description.Trim());
            foreach (var itemToAddToList in itemsToExport.Except(existingItemsInList, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogTrace($"Adding item [{itemToAddToList}] to SmallLister list [{listId}]");
                await _smlClient.AddItemAsync(user.SmallListerToken, listId, itemToAddToList);
            }
        }

        private async Task ImportFromSmlAsync(UserAccount user, string listId)
        {
            var list = await _smlClient.GetListAsync(user.SmallListerToken, listId);
            if (list == null)
                return;
            var itemsToImport = list.Items.Select(x => x.Description);

            var currentList = (await _shoppingListRepository.GetActiveItemsAsync(user)).Select(i => i.Ingredient.Description);
            foreach (var itemToAddToShoppingList in itemsToImport.Except(currentList, StringComparer.InvariantCultureIgnoreCase))
            {
                _logger.LogTrace($"Adding item [{itemToAddToShoppingList}] to shopping list");
                await _shoppingListRepository.AddNewIngredientAsync(user, itemToAddToShoppingList);
            }
        }
    }
}