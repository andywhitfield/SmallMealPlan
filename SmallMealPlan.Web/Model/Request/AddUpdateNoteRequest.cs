using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Web.Model.Request;

public class AddUpdateNoteRequest
{
    [MaxLength(10000)]
    public string? NoteText { get; set; }
}