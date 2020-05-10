using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Data;
using SmallMealPlan.Web.Model.Request;

namespace SmallMealPlan.Web.Controllers
{
    [ApiController]
    [Authorize]
    public class NotesApiController : ControllerBase
    {
        private readonly ILogger<NotesApiController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly INoteRepository _noteRepository;

        public NotesApiController(ILogger<NotesApiController> logger,
            IUserAccountRepository userAccountRepository,
            INoteRepository noteRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _noteRepository = noteRepository;
        }

        [HttpPut("~/api/note")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddOrUpdate(AddUpdateNoteRequest addUpdateNote)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            await _noteRepository.AddOrUpdateAsync(user, string.IsNullOrWhiteSpace(addUpdateNote.NoteText) ? "" : addUpdateNote.NoteText);
            return NoContent();
        }
    }
}