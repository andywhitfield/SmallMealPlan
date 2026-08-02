using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallMealPlan.Data;
using SmallMealPlan.Web.Model.Notes;

namespace SmallMealPlan.Web.Controllers;

[Authorize]
public class NotesController(
    IUserAccountRepository userAccountRepository,
    INoteRepository noteRepository,
    INoteHistoryRepository noteHistoryRepository)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string? find)
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        return View(new IndexViewModel(HttpContext)
        {
            Notes = noteRepository.GetAllAsync(user, find).Select(n => new NoteViewModel(HttpContext, n)),
            SortedManually = user.NoteSortOrdering == INoteRepository.SortedManually
        });
    }

    [HttpGet("~/notes/{noteId}")]
    public async Task<IActionResult> Note(int noteId)
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        var note = await noteRepository.GetAsync(noteId);
        if (note?.UserAccountId != user.UserAccountId)
            return NotFound();
        return View(new NoteViewModel(HttpContext, note));
    }

    [HttpPost("~/notes/{noteId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Note([FromRoute] int noteId, [FromForm] string? title, [FromForm] string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return Redirect($"~/notes/{noteId}");

        var user = await userAccountRepository.GetUserAccountAsync(User);
        var note = await noteRepository.GetAsync(noteId);
        if (note?.UserAccountId != user.UserAccountId)
            return NotFound();
        await noteRepository.UpdateAsync(note, title, notes);
        return Redirect("~/notes");
    }

    [HttpGet("~/notes/history/{noteId}")]
    public async Task<IActionResult> History(int noteId)
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        var note = await noteRepository.GetAsync(noteId);
        if (note?.UserAccountId != user.UserAccountId)
            return NotFound();
        return View(new HistoryViewModel(HttpContext, new(HttpContext, note))
        {
            Histories = noteHistoryRepository.GetByNoteIdAsync(note.NoteId).OrderByDescending(nh => nh.NoteHistoryId).Select(nh => new NoteHistoryViewModel(nh))
        });
    }

    [HttpGet("~/notes/add")]
    public IActionResult NewNote()
        => View(new NewNoteViewModel(HttpContext));

    [HttpPost("~/notes/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NewNote([FromForm] string? title, [FromForm] string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return Redirect($"~/notes/add");

        var user = await userAccountRepository.GetUserAccountAsync(User);
        await noteRepository.AddAsync(user, title, notes);
        return Redirect("~/notes");
    }

    [HttpPost("~/notes/delete/{noteId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNote(int noteId)
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        var note = await noteRepository.GetAsync(noteId);
        if (note?.UserAccountId != user.UserAccountId)
            return NotFound();
        await noteRepository.DeleteAsync(note);
        return Redirect("~/notes");
    }

    [HttpPost("~/notes/sort")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sort()
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        // for now, we have 2 sort options: latest and manual. If not manually ordered, the sort property should be null.
        user.NoteSortOrdering = null;
        await userAccountRepository.UpdateAsync(user);
        return Redirect("~/notes");
    }
}
