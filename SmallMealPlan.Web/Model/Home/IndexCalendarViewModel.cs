namespace SmallMealPlan.Web.Model.Home;

public class IndexCalendarViewModel : BaseViewModel
{
    public IndexCalendarViewModel(HttpContext context) : base(context)
    {
        SelectedArea = SmpArea.Planner;
    }

    // TODO
    public string InitialDate => "2026-11-02";
}
