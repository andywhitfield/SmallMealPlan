using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallMealPlan.Web.Model.Notes;

namespace SmallMealPlan.Web.Controllers;

[Authorize]
public class NotesController : Controller
{
    public async Task<IActionResult> Index()
        => View(new IndexViewModel(HttpContext));
}