using System;
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
        private readonly IPlannerMealRepository _plannerMealRepository;

        public HomeController(ILogger<HomeController> logger,
            IUserAccountRepository userAccountRepository,
            IPlannerMealRepository plannerMealRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _plannerMealRepository = plannerMealRepository;
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

            var plannerDayViewModels = Enumerable.Range(0, 7).Select(d => new PlannerDayViewModel
            {
                Day = monday.AddDays(d)
            }).ToList();

            var plannerMeals = await _plannerMealRepository.GetPlannerMealsAsync(
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
        [HttpGet("~/planner/{date}/add")]
        public IActionResult Planner(string date) => View(new PlannerViewModel(HttpContext, date.ParseDateOrToday()));

        [Authorize]
        [HttpPost("~/planner/{date}/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToPlanner([FromRoute] string date, [FromForm] AddToPlannerRequest addModel)
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