using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Web.Model.Notes;

namespace SmallMealPlan.Web.Controllers
{
    public class NotesController : Controller
    {
        private readonly ILogger<NotesController> _logger;

        public NotesController(ILogger<NotesController> logger)
        {
            _logger = logger;
        }

        //[Authorize]
        public IActionResult Index() => View(new IndexViewModel());
    }
}