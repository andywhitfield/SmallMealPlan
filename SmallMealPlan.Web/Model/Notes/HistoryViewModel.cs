namespace SmallMealPlan.Web.Model.Notes;

public class HistoryViewModel : BaseViewModel
{
    public HistoryViewModel(HttpContext context, NoteViewModel note) : base(context)
    {
        SelectedArea = SmpArea.Notes;
        Note = note;
    }

    public NoteViewModel Note { get; }
    public string NoteCreatedDateTime => Note.Note.CreatedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
    public IAsyncEnumerable<NoteHistoryViewModel> Histories { get; set; } = AsyncEnumerable.Empty<NoteHistoryViewModel>();
}
