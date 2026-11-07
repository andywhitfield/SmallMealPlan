namespace SmallMealPlan.Web.Model.Home;

public class IndexCalendarViewModel : BaseViewModel
{
    public IndexCalendarViewModel(HttpContext context, DateTime date) : base(context)
    {
        SelectedArea = SmpArea.Planner;
        InitialDate = date.ToString("yyyy-MM-dd");
    }

    public string InitialDate { init; get; }
}
