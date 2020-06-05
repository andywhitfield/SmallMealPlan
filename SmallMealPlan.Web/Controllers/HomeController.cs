using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.Model;
using SmallMealPlan.Web.Model;
using SmallMealPlan.Web.Model.Home;
using SmallMealPlan.Web.Model.Request;

namespace SmallMealPlan.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IPlannerMealRepository _plannerMealRepository;
        private readonly IMealRepository _mealRepository;

        public HomeController(ILogger<HomeController> logger,
            IUserAccountRepository userAccountRepository,
            IPlannerMealRepository plannerMealRepository,
            IMealRepository mealRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _plannerMealRepository = plannerMealRepository;
            _mealRepository = mealRepository;
        }

        [Authorize]
        [HttpGet("~/")]
        [HttpGet("~/planner")]
        [HttpGet("~/planner/{date}")]
        public async Task<IActionResult> Index(string date)
        {
            var monday = date.ParseDateOrToday();
            if (monday.DayOfWeek != DayOfWeek.Monday)
            {
                var daysSinceMonday = (monday.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)monday.DayOfWeek) - 1;
                monday = monday - TimeSpan.FromDays(daysSinceMonday);
            }
            monday = monday.Date;

            var plannerDayViewModels = Enumerable.Range(0, 14).Select(d => new PlannerDayViewModel
            {
                Day = monday.AddDays(d)
            }).ToList();

            var plannerMeals = await _plannerMealRepository.GetPlannerMealsAsync(
                await _userAccountRepository.GetUserAccountAsync(User), monday, monday.AddDays(14));

            foreach (var plannerMeal in plannerMeals)
            {
                var viewModel = plannerDayViewModels.FirstOrDefault(vm => vm.Day == plannerMeal.Date);
                if (viewModel == null)
                {
                    _logger.LogWarning($"Could not add planner meal {plannerMeal.PlannerMealId} with date {plannerMeal.Date}");
                    continue;
                }
                viewModel.Meals.Add(new PlannerDayMealViewModel(plannerMeal.PlannerMealId)
                {
                    Name = plannerMeal.Meal.Description,
                    Notes = plannerMeal.Meal.Notes,
                    Ingredients = plannerMeal.Meal.Ingredients?.OrderBy(mi => mi.SortOrder).Select(i => i.Ingredient.Description) ?? Enumerable.Empty<string>()
                });
            }

            return View(new IndexViewModel(HttpContext)
            {
                PreviousWeekStart = monday.AddDays(-7),
                NextWeekStart = monday.AddDays(7),
                PreviousWeek = $"{monday.AddDays(-7).ToString(BaseViewModel.ShortDateFormat)} - {monday.AddDays(-1).ToString(BaseViewModel.ShortDateFormat)}",
                NextWeek = $"{monday.AddDays(7).ToString(BaseViewModel.ShortDateFormat)} - {monday.AddDays(13).ToString(BaseViewModel.ShortDateFormat)}",
                Days = plannerDayViewModels
            });
        }

        [Authorize]
        [HttpGet("~/planner/{date}/add")]
        public async Task<IActionResult> Planner(string date, [FromQuery] int? pageNumber, [FromQuery] string sort, [FromQuery] string filter)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            List<Meal> meals;
            int page;
            int pageCount;
            var sortByName = sort == Pagination.SortByName;
            if (sortByName)
                (meals, page, pageCount) = await _mealRepository.GetMealsByNameAsync(user, pageNumber ?? 1, filter);
            else
                (meals, page, pageCount) = await _mealRepository.GetMealsByMostRecentlyUsedAsync(user, pageNumber ?? 1, filter);

            return View(new PlannerViewModel(HttpContext, date.ParseDateOrToday())
            {
                Pagination = new Pagination(page, pageCount, sort, filter),
                Meals = meals
                .Select(m => new PlannerDayMealViewModel(m.MealId)
                {
                    Name = m.Description,
                    Notes = m.Notes,
                    Ingredients = m.Ingredients?.OrderBy(mi => mi.SortOrder).Select(i => i.Ingredient.Description) ?? Enumerable.Empty<string>()
                })
                .ToList()
            });
        }

        [Authorize]
        [HttpGet("~/planner/{date}/add/{mealId}")]
        public async Task<IActionResult> AddExistingMealToPlanner(string date, int mealId)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var meal = await _mealRepository.GetAsync(mealId);
            if (meal.User != user)
                return BadRequest();
            await _plannerMealRepository.AddMealToPlannerAsync(user, date.ParseDateOrToday(), meal);
            return Redirect($"~/planner/{date}");
        }

        [Authorize]
        [HttpPost("~/planner/{date}/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewMealToPlanner([FromRoute] string date, [FromForm] AddToPlannerRequest addModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            await _plannerMealRepository.AddNewMealToPlannerAsync(user, date.ParseDateOrToday(), addModel.Description, addModel.Ingredients?.Split('\n').Where(i => !string.IsNullOrWhiteSpace(i)) ?? new string[0], addModel.Notes);
            return Redirect($"~/planner/{date}");
        }

        [Authorize]
        [HttpPost("~/planner/{date}/delete/{plannerMealId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFromPlanner([FromRoute] string date, [FromRoute] int plannerMealId)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            await _plannerMealRepository.DeleteMealFromPlannerAsync(user, plannerMealId);
            return Redirect($"~/planner/{date}");
        }

        [Authorize]
        [HttpGet("~/planner/{date}/edit/{plannerMealId}")]
        public async Task<IActionResult> Edit([FromRoute] string date, [FromRoute] int plannerMealId)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var plannerMeal = await _plannerMealRepository.GetAsync(plannerMealId);
            if (plannerMeal.User != user)
                return BadRequest();

            return View(new PlannerEditMealViewModel(HttpContext, date.ParseDateOrToday(), plannerMeal.PlannerMealId)
            {
                Name = plannerMeal.Meal.Description,
                Ingredients = plannerMeal.Meal.Ingredients == null ? null : string.Join('\n', plannerMeal.Meal.Ingredients.OrderBy(mi => mi.SortOrder).Select(mi => mi.Ingredient.Description)),
                Notes = plannerMeal.Meal.Notes
            });
        }

        [Authorize]
        [HttpPost("~/planner/{date}/edit/{plannerMealId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEdit([FromRoute] string date, [FromRoute] int plannerMealId, [FromForm] EditPlannerMealRequest editModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (editModel.Cancel ?? false)
                return Redirect($"~/planner/{date}");

            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var plannerMeal = await _plannerMealRepository.GetAsync(plannerMealId);
            if (plannerMeal.User != user)
                return BadRequest();

            var ingredients = editModel.Ingredients?.Split('\n').Where(i => !string.IsNullOrWhiteSpace(i)) ?? new string[0];

            if (editModel.SaveAsNew ?? false)
            {
                await _plannerMealRepository.DeleteMealFromPlannerAsync(user, plannerMealId);
                await _plannerMealRepository.AddNewMealToPlannerAsync(user, plannerMeal.Date, editModel.Description, ingredients, editModel.Notes);
            }
            else
            {
                await _plannerMealRepository.UpdateMealPlannerAsync(user, plannerMealId, plannerMeal.Date, editModel.Description, ingredients, editModel.Notes);
            }

            return Redirect($"~/planner/{date}");
        }

        public IActionResult Error() => View(new ErrorViewModel(HttpContext));

        [HttpGet("~/signin")]
        public IActionResult SignIn() => View("Login", new LoginViewModel(HttpContext));

        [HttpPost("~/signin")]
        [ValidateAntiForgeryToken]
        public IActionResult SignInChallenge() => Challenge(new AuthenticationProperties { RedirectUri = "/signedin" }, OpenIdConnectDefaults.AuthenticationScheme);

        [Authorize]
        [HttpGet("~/signedin")]
        public async Task<IActionResult> SignedIn()
        {
            if (await _userAccountRepository.GetUserAccountOrNullAsync(User) == null)
                await _userAccountRepository.CreateNewUserAsync(User);
            return Redirect("~/");
        }

        [HttpGet("~/signout"), HttpPost("~/signout")]
        public IActionResult SignOut()
        {
            HttpContext.Session.Clear();
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}