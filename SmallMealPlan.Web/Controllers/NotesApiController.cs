using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallMealPlan.Data;
using SmallMealPlan.Web.Model.Notes;
using SmallMealPlan.Web.Model.Request;

namespace SmallMealPlan.Web.Controllers;

[ApiController]
[Authorize]
public class NotesApiController(
    ILogger<ShoppingListApiController> logger,
    IUserAccountRepository userAccountRepository,
    INoteRepository noteRepository)
    : ControllerBase
{
    [HttpGet("~/api/notes/{noteId}/info/{infoType}")]
    public async Task<IActionResult> NoteInfo(int noteId, string infoType)
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        var note = await noteRepository.GetAsync(noteId);
        if (note?.UserAccountId != user.UserAccountId)
            return NotFound();
        if (infoType == "details")
            return Ok(new { title = note.Title ?? "", note = note.NoteText });

        return Ok(new { title = new NoteViewModel(HttpContext, note).TitleForDisplay });
    }

    [HttpPut("~/api/notes/{noteId}/move")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Move(int noteId, NoteMoveRequest noteMoveRequest)
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        logger.LogTrace("Moving note {NoteId} to after {SortOrderPreviousNoteId}", noteId, noteMoveRequest.SortOrderPreviousNoteId);
        await noteRepository.ReorderAsync(user, noteId, noteMoveRequest.SortOrderPreviousNoteId);
        return NoContent();
    }
}