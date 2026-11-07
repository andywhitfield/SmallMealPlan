namespace SmallMealPlan.Web.Model.Home;

public class IndexViewModel : BaseViewModel
{
    public IndexViewModel(HttpContext context) : base(context)
    {
        SelectedArea = SmpArea.Planner;
    }

    public required DateTime WeekStart { get; set; }
    public DateTime? PreviousWeekStart { get; set; }
    public DateTime? NextWeekStart { get; set; }
    public string? PreviousWeek { get; set; }
    public string? NextWeek { get; set; }
    public IEnumerable<PlannerDayViewModel>? Days { get; set; }
}