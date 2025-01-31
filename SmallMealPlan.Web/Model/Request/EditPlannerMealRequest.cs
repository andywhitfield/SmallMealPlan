using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Web.Model.Request;

public class EditPlannerMealRequest
{
    [Required]
    public required string Description { get; set; }
    public string? Ingredients { get; set; }
    public string? Notes { get; set; }
    public string? DateNotes { get; set; }
    public bool? Cancel { get; set; }
    public bool? Save { get; set; }
    public bool? SaveAsNew { get; set; }
}