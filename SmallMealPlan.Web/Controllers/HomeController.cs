using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.Web.Model;
using SmallMealPlan.Web.Model.Home;
using SmallMealPlan.Web.Model.Request;

namespace SmallMealPlan.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IMealPlannerRepository _mealPlannerRepository;

        public HomeController(ILogger<HomeController> logger,
            IUserAccountRepository userAccountRepository,
            IMealPlannerRepository mealPlannerRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _mealPlannerRepository = mealPlannerRepository;
        }

        private DateTime ParseDateOrToday(string date) => DateTime.TryParseExact(date, "yyyyMMdd", null, DateTimeStyles.AssumeUniversal, out var dt) ? dt : DateTime.Today;

        [Authorize]
        [HttpGet("~/")]
        [HttpGet("~/planner")]
        [HttpGet("~/planner/{date}")]
        public async Task<IActionResult> Index(string date)
        {
            var monday = ParseDateOrToday(date);
            if (monday.DayOfWeek != DayOfWeek.Monday)
            {
                var daysSinceMonday = (monday.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)monday.DayOfWeek) - 1;
                monday = monday - TimeSpan.FromDays(daysSinceMonday);
            }
            monday = monday.Date;

            var plannerDayViewModels = Enumerable.Range(0, 7).Select(d => new PlannerDayViewModel
            {
                Day = monday.AddDays(d)
            }).ToList();

            var plannerMeals = await _mealPlannerRepository.GetPlannerMealsAsync(
                await _userAccountRepository.GetUserAccountAsync(User), monday, monday.AddDays(7));

            foreach (var plannerMeal in plannerMeals)
            {
                var viewModel = plannerDayViewModels.FirstOrDefault(vm => vm.Day == plannerMeal.Date);
                if (viewModel == null)
                {
                    _logger.LogWarning($"Could not add planner meal {plannerMeal.PlannerMealId} with date {plannerMeal.Date}");
                    continue;
                }
                viewModel.Meals.Add(new PlannerDayMealViewModel(plannerMeal.PlannerMealId) {
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
        [HttpGet("~/planner/add/{date}")]
        public IActionResult Planner(string date) => View(new PlannerViewModel(HttpContext, ParseDateOrToday(date)));

        [Authorize]
        [HttpPost("~/planner/add/{date}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToPlanner([FromRoute] string date, [FromForm] AddToPlannerRequest addModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var userAccount = await _userAccountRepository.GetUserAccountAsync(User);
            await _mealPlannerRepository.AddNewMealToPlannerAsync(userAccount, ParseDateOrToday(date), addModel.Description, addModel.Ingredients?.Split('\n').Where(i => !string.IsNullOrWhiteSpace(i)) ?? new string[0], addModel.Notes);
            return Redirect($"~/planner/{date}");
        }

        public IActionResult Error() => View(new ErrorViewModel(HttpContext));

        [HttpGet("~/signin")]
        public IActionResult SignIn() => View("Login", new LoginViewModel(HttpContext));

        [HttpPost("~/signin")]
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