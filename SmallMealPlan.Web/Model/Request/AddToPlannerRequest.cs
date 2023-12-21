using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Web.Model.Request;

public class AddToPlannerRequest
{
    [Required]
    public required string Description { get; set; }
    public string? Ingredients { get; set; }
    public string? Notes { get; set; }
}