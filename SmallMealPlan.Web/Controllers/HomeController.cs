using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SmallMealPlan.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        //[Authorize]
        public IActionResult Index() => View();
        public IActionResult Error() => View();        
    }
}