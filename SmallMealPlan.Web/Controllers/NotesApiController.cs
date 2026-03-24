using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallMealPlan.Data;
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