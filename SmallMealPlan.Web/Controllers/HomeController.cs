using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Web.Model;
using SmallMealPlan.Web.Model.Home;

namespace SmallMealPlan.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Authorize]
        [HttpGet("/")]
        [HttpGet("/planner")]
        [HttpGet("/planner/{date}")]
        public IActionResult Index(string date)
        {
            if (!DateTime.TryParseExact(date, "yyyyMMdd", null, DateTimeStyles.AssumeUniversal, out var monday))
                monday = DateTime.UtcNow.Date;

            if (monday.DayOfWeek != DayOfWeek.Monday)
            {
                var daysSinceMonday = (monday.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)monday.DayOfWeek) - 1;
                monday = monday - TimeSpan.FromDays(daysSinceMonday);
            }
            monday = monday.Date;

            return View(new IndexViewModel(HttpContext)
            {
                PreviousWeekStart = monday.AddDays(-7),
                NextWeekStart = monday.AddDays(7),
                PreviousWeek = $"{monday.AddDays(-7).ToString(BaseViewModel.ShortDateFormat)} - {monday.AddDays(-1).ToString(BaseViewModel.ShortDateFormat)}",
                NextWeek = $"{monday.AddDays(7).ToString(BaseViewModel.ShortDateFormat)} - {monday.AddDays(13).ToString(BaseViewModel.ShortDateFormat)}",
                Days = Enumerable.Range(0, 7).Select(d => new PlannerDayViewModel
                {
                    Day = monday.AddDays(d),
                    Meals =
                    d == 0 ? new[] { new PlannerDayMealViewModel(1) {
                        Name = "Spaghetti",
                        Ingredients = new[] {
                            "Wholewheat spaghetti",
                            "Onion",
                            "Tinned tomatoes",
                            "Tinned fish"
                        },
                        Notes = "Quick & easy"
                    }} :
                    d == 1 ? new[] { new PlannerDayMealViewModel(2) {
                        Name = "Pasta bake",
                        Notes = "Long but easy"
                    }} :
                    d == 2 ? new[] { new PlannerDayMealViewModel(3) {
                        Name = "Pizza"
                    }} : Enumerable.Empty<PlannerDayMealViewModel>()
                })
            });
        }

        [Authorize]
        [HttpGet("/planner/add/{date}")]
        public IActionResult Planner(string date) => View(new PlannerViewModel(HttpContext));

        public IActionResult Error() => View(new ErrorViewModel(HttpContext));

        [HttpGet("~/signin")]
        public IActionResult SignIn() => View("Login", new LoginViewModel(HttpContext));

        [HttpPost("~/signin")]
        public IActionResult SignInChallenge()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("~/signout"), HttpPost("~/signout")]
        public IActionResult SignOut()
        {
            HttpContext.Session.Clear();
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}