using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Web.Model.ShoppingList;

namespace SmallMealPlan.Web.Controllers
{
    public class ShoppingListController : Controller
    {
        private readonly ILogger<ShoppingListController> _logger;

        public ShoppingListController(ILogger<ShoppingListController> logger)
        {
            _logger = logger;
        }

        //[Authorize]
        public IActionResult Index() => View(new IndexViewModel());
    }
}