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

    public string TitleForDisplay => Note.Title ?? Title(Note.NoteText);

    public string UpdatedDate => (Note.LastUpdateDateTime ?? Note.CreatedDateTime).ToString("yyyy-MM-dd HH:mm:ss");

    private static string Title(string? text)
        => string.IsNullOrEmpty(text) ? "" : text.Split('\n', '\r', '\t')[0];
}
