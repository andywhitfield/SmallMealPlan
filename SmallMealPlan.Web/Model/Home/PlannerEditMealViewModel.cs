namespace SmallMealPlan.Web.Model.Home;

public class PlannerEditMealViewModel : BaseViewModel
{
    public PlannerEditMealViewModel(HttpContext context, DateTime day, int plannerMealId) : base(context)
    {
        Day = day;
        PlannerMealId = plannerMealId;
        SelectedArea = SmpArea.Planner;
    }

    public DateTime Day { get; }
    public int PlannerMealId { get; }
    public string? Name { get; set; }
    public string? Ingredients { get; set; }
    public string? Notes { get; set; }
    public string? DateNotes { get; set; }
}