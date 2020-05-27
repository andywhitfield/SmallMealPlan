using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.Model;
using SmallMealPlan.Web.Model.Meals;
using SmallMealPlan.Web.Model.Request;

namespace SmallMealPlan.Web.Controllers
{
    [Authorize]
    public class MealsController : Controller
    {
        private readonly ILogger<MealsController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IMealRepository _mealRepository;

        public MealsController(ILogger<MealsController> logger,
            IUserAccountRepository userAccountRepository,
            IMealRepository mealRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _mealRepository = mealRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? pageNumber, [FromQuery] string sort)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            List<Meal> meals;
            int page;
            int pageCount;
            var sortByName = sort == Pagination.SortByName;
            if (sortByName)
                (meals, page, pageCount) = await _mealRepository.GetMealsByNameAsync(user, pageNumber ?? 1);
            else
                (meals, page, pageCount) = await _mealRepository.GetMealsByMostRecentlyUsedAsync(user, pageNumber ?? 1);

            return View(new IndexViewModel(HttpContext)
            {
                Pagination = new Pagination(page, pageCount, sortByName ? Pagination.SortByName : Pagination.SortByRecentlyUsed),
                Meals = meals
                .Select(m => new Model.Home.PlannerDayMealViewModel(m.MealId)
                {
                    Name = m.Description,
                    Notes = m.Notes,
                    Ingredients = m.Ingredients?.OrderBy(mi => mi.SortOrder).Select(i => i.Ingredient.Description) ?? Enumerable.Empty<string>()
                })
                .ToList()
            });
        }

        [HttpPost("~/meals")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewMeal([FromForm] AddMealRequest addModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            await _mealRepository.AddNewMealAsync(user, addModel.Description, addModel.Ingredients?.Split('\n').Where(i => !string.IsNullOrWhiteSpace(i)) ?? new string[0], addModel.Notes);
            return Redirect("~/meals");
        }

        [HttpGet("~/meal/edit/{mealId}")]
        public async Task<IActionResult> Edit([FromRoute] int mealId)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var meal = await _mealRepository.GetAsync(mealId);
            if (meal.User != user)
                return BadRequest();

            return View(new EditMealViewModel(HttpContext, meal.MealId)
            {
                Name = meal.Description,
                Ingredients = meal.Ingredients == null ? null : string.Join('\n', meal.Ingredients.OrderBy(mi => mi.SortOrder).Select(mi => mi.Ingredient.Description)),
                Notes = meal.Notes
            });
        }

        [HttpPost("~/meal/edit/{mealId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEdit([FromRoute] int mealId, [FromForm] EditPlannerMealRequest editModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (editModel.Cancel ?? false)
                return Redirect($"~/meals");

            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var meal = await _mealRepository.GetAsync(mealId);
            if (meal.User != user)
                return BadRequest();

            var ingredients = editModel.Ingredients?.Split('\n').Where(i => !string.IsNullOrWhiteSpace(i)) ?? new string[0];

            if (editModel.SaveAsNew ?? false)
                await _mealRepository.AddNewMealAsync(user, editModel.Description, ingredients, editModel.Notes);
            else
                await _mealRepository.UpdateMealAsync(user, meal, editModel.Description, ingredients, editModel.Notes);

            return Redirect($"~/meals");
        }

        [HttpPost("~/meal/delete/{mealId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMeal([FromRoute] int mealId)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var meal = await _mealRepository.GetAsync(mealId);
            await _mealRepository.DeleteMealAsync(user, meal);
            return Redirect("~/meals");
        }
    }
}