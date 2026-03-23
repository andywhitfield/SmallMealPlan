namespace SmallMealPlan.Web.Model.Notes;

public class NoteViewModel : BaseViewModel
{
    public NoteViewModel(HttpContext context) : base(context)
    {
        SelectedArea = SmpArea.Notes;
    }
}