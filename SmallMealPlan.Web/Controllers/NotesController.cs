using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallMealPlan.Data;
using SmallMealPlan.Web.Model.Notes;

namespace SmallMealPlan.Web.Controllers;

[Authorize]
public class NotesController(
    IUserAccountRepository userAccountRepository,
    INoteRepository noteRepository)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        return View(new IndexViewModel(HttpContext)
        {
            Notes = noteRepository.GetAllAsync(user).Select(n => new NoteViewModel(HttpContext, n))
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

    [HttpGet("~/notes/add")]
    public IActionResult NewNote()
        => View(new NewNoteViewModel(HttpContext));

    [HttpPost("~/notes/add")]
    public async Task<IActionResult> NewNote([FromForm] string? title, [FromForm] string notes)
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        await noteRepository.AddAsync(user, title, notes);
        return Redirect("~/notes");
    }

    [HttpPost("~/notes/delete/{noteId}")]
    public async Task<IActionResult> DeleteNote(int noteId)
        //=> View(new NoteViewModel(HttpContext));
        => throw new NotImplementedException("TODO");
}
