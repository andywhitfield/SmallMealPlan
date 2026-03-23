using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallMealPlan.Web.Model.Notes;

namespace SmallMealPlan.Web.Controllers;

[Authorize]
public class NotesController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
        => View(new IndexViewModel(HttpContext));

    [HttpGet("~/notes/{noteId}")]
    public async Task<IActionResult> Note(int noteId)
        => View(new NoteViewModel(HttpContext));
}
