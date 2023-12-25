using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Web.Model.Request;

public class PlannerMealMoveRequest
{
    [Required]
    public required string Date { get; set; }
    public int? SortOrderPreviousPlannerMealId { get; set; }
}