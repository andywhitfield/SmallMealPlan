using SmallMealPlan.Model;

namespace SmallMealPlan.Web.Model.Notes;

public class NoteViewModel : BaseViewModel
{
    public NoteViewModel(HttpContext context, Note note) : base(context)
    {
        SelectedArea = SmpArea.Notes;
        Note = note;
    }

    public Note Note { get; }

    public string TitleForDisplay => Note.Title ?? Note.NoteText; // TODO: get the first line of NoteText (or perhaps up to a max length)
}
