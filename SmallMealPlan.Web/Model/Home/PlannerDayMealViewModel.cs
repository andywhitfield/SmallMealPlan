namespace SmallMealPlan.Web.Model.Home;

public class PlannerDayMealViewModel(int id)
{
    public int Id { get; } = id;

    public string? Name { get; set; }

    public bool HasIngredients => Ingredients?.Any() ?? false;
    public IEnumerable<string>? Ingredients { get; set; }

    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);
    public string? Notes { get; set; }
    public bool HasDateNotes => !string.IsNullOrWhiteSpace(DateNotes);
    public string? DateNotes { get; set; }
}