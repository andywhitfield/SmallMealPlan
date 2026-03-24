namespace SmallMealPlan.Web.Model.Notes;

public class NewNoteViewModel : BaseViewModel
{
    public NewNoteViewModel(HttpContext context) : base(context)
        => SelectedArea = SmpArea.Notes;
}
