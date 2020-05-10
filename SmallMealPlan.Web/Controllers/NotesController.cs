using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.Web.Model.Notes;

namespace SmallMealPlan.Web.Controllers
{
    public class NotesController : Controller
    {
        private readonly ILogger<NotesController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly INoteRepository _noteRepository;

        public NotesController(ILogger<NotesController> logger,
            IUserAccountRepository userAccountRepository,
            INoteRepository noteRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _noteRepository = noteRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            return View(new IndexViewModel(HttpContext)
            {
                Notes = await _noteRepository.GetAsync(user)
            });
        }
    }
}