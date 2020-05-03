using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Web.Model.Meals;

namespace SmallMealPlan.Web.Controllers
{
    public class MealsController : Controller
    {
        private readonly ILogger<MealsController> _logger;

        public MealsController(ILogger<MealsController> logger)
        {
            _logger = logger;
        }

        //[Authorize]
        public IActionResult Index() => View(new IndexViewModel());
    }
}