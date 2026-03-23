namespace SmallMealPlan.Web.Model.Notes;

public class IndexViewModel : BaseViewModel
{
    public IndexViewModel(HttpContext context) : base(context)
        => SelectedArea = SmpArea.Notes;

    public IAsyncEnumerable<NoteViewModel> Notes { get; set; } = AsyncEnumerable.Empty<NoteViewModel>();
}