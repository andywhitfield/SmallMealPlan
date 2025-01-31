using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallMealPlan.Data;
using SmallMealPlan.Model;
using SmallMealPlan.Web.Authorisation;
using SmallMealPlan.Web.Model;
using SmallMealPlan.Web.Model.Home;
using SmallMealPlan.Web.Model.Request;

namespace SmallMealPlan.Web.Controllers;

public class HomeController(ILogger<HomeController> logger,
    IUserAccountRepository userAccountRepository,
    IPlannerMealRepository plannerMealRepository,
    IMealRepository mealRepository,
    IAuthorisationHandler authorisationHandler)
    : Controller
{
    [Authorize]
    [HttpGet("~/")]
    [HttpGet("~/planner")]
    [HttpGet("~/planner/{date}")]
    public async Task<IActionResult> Index(string? date)
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

        var plannerMeals = await plannerMealRepository.GetPlannerMealsAsync(
            await userAccountRepository.GetUserAccountAsync(User), monday, monday.AddDays(14));

        foreach (var plannerMeal in plannerMeals)
        {
            var viewModel = plannerDayViewModels.FirstOrDefault(vm => vm.Day == plannerMeal.Date);
            if (viewModel == null)
            {
                logger.LogWarning($"Could not add planner meal {plannerMeal.PlannerMealId} with date {plannerMeal.Date}");
                continue;
            }
            viewModel.Meals.Add(new PlannerDayMealViewModel(plannerMeal.PlannerMealId)
            {
                Name = plannerMeal.Meal.Description,
                Notes = plannerMeal.Meal.Notes,
                DateNotes = plannerMeal.Notes,
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
    public async Task<IActionResult> Planner(string date, [FromQuery] int? pageNumber, [FromQuery] string? sort, [FromQuery] string? filter)
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        List<Meal> meals;
        int page;
        int pageCount;
        var sortByName = sort == Pagination.SortByName;
        if (sortByName)
            (meals, page, pageCount) = await mealRepository.GetMealsByNameAsync(user, pageNumber ?? 1, filter);
        else
            (meals, page, pageCount) = await mealRepository.GetMealsByMostRecentlyUsedAsync(user, pageNumber ?? 1, filter);

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
        var user = await userAccountRepository.GetUserAccountAsync(User);
        var meal = await mealRepository.GetAsync(mealId);
        if (meal.User != user)
            return BadRequest();
        await plannerMealRepository.AddMealToPlannerAsync(user, date.ParseDateOrToday(), meal);
        return Redirect($"~/planner/{date}");
    }

    [Authorize]
    [HttpPost("~/planner/{date}/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNewMealToPlanner([FromRoute] string date, [FromForm] AddToPlannerRequest addModel)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        var user = await userAccountRepository.GetUserAccountAsync(User);
        await plannerMealRepository.AddNewMealToPlannerAsync(user, date.ParseDateOrToday(), addModel.Description.Trim(), addModel.Ingredients?.Split('\n', StringSplitOptions.TrimEntries).Where(i => !string.IsNullOrWhiteSpace(i)) ?? Array.Empty<string>(), addModel.Notes?.Trim(), addModel.DateNotes?.Trim());
        return Redirect($"~/planner/{date}");
    }

    [Authorize]
    [HttpPost("~/planner/{date}/delete/{plannerMealId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFromPlanner([FromRoute] string date, [FromRoute] int plannerMealId)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        var user = await userAccountRepository.GetUserAccountAsync(User);
        await plannerMealRepository.DeleteMealFromPlannerAsync(user, plannerMealId);
        return Redirect($"~/planner/{date}");
    }

    [Authorize]
    [HttpGet("~/planner/{date}/edit/{plannerMealId}")]
    public async Task<IActionResult> Edit([FromRoute] string date, [FromRoute] int plannerMealId)
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        var plannerMeal = await plannerMealRepository.GetAsync(plannerMealId);
        if (plannerMeal.User != user)
            return BadRequest();

        return View(new PlannerEditMealViewModel(HttpContext, date.ParseDateOrToday(), plannerMeal.PlannerMealId)
        {
            Name = plannerMeal.Meal.Description,
            Ingredients = plannerMeal.Meal.Ingredients == null ? null : string.Join('\n', plannerMeal.Meal.Ingredients.OrderBy(mi => mi.SortOrder).Select(mi => mi.Ingredient.Description)),
            Notes = plannerMeal.Meal.Notes,
            DateNotes = plannerMeal.Notes
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

        var user = await userAccountRepository.GetUserAccountAsync(User);
        var plannerMeal = await plannerMealRepository.GetAsync(plannerMealId);
        if (plannerMeal.User != user)
            return BadRequest();

        var ingredients = editModel.Ingredients?.Split('\n', StringSplitOptions.TrimEntries).Where(i => !string.IsNullOrWhiteSpace(i)) ?? new string[0];

        if (editModel.SaveAsNew ?? false)
        {
            await plannerMealRepository.DeleteMealFromPlannerAsync(user, plannerMealId);
            await plannerMealRepository.AddNewMealToPlannerAsync(user, plannerMeal.Date, editModel.Description.Trim(), ingredients, editModel.Notes?.Trim(), editModel.DateNotes?.Trim());
        }
        else
        {
            await plannerMealRepository.UpdateMealPlannerAsync(user, plannerMealId, plannerMeal.Date, editModel.Description.Trim(), ingredients, editModel.Notes?.Trim(), editModel.DateNotes?.Trim());
        }

        return Redirect($"~/planner/{date}");
    }

    public IActionResult Error() => View(new ErrorViewModel(HttpContext));

    [HttpGet("~/signin")]
    public IActionResult Signin([FromQuery] string? returnUrl) => View("Login", new LoginViewModel(HttpContext, returnUrl));

    [HttpPost("~/signin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Signin([FromForm] string? returnUrl, [FromForm, Required] string email, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("Login", new LoginViewModel(HttpContext, returnUrl));

        var (isReturningUser, verifyOptions) = await authorisationHandler.HandleSigninRequest(email, cancellationToken);
        return View("LoginVerify", new LoginVerifyViewModel(HttpContext, returnUrl, email, isReturningUser, verifyOptions));
    }

    [HttpPost("~/signin/verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SigninVerify(
        [FromForm] string? returnUrl,
        [FromForm, Required] string email,
        [FromForm, Required] string verifyOptions,
        [FromForm, Required] string verifyResponse,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Redirect("~/signin");

        var isValid = await authorisationHandler.HandleSigninVerifyRequest(HttpContext, email, verifyOptions, verifyResponse, cancellationToken);
        if (isValid)
        {
            var redirectUri = "~/";
            if (!string.IsNullOrEmpty(returnUrl) && Uri.TryCreate(returnUrl, UriKind.Relative, out var uri))
                redirectUri = uri.ToString();

            return Redirect(redirectUri);
        }
        
        return Redirect("~/signin");
    }

    [HttpGet("~/signout"), HttpPost("~/signout")]
    public async Task<IActionResult> Signout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("~/");
    }
}