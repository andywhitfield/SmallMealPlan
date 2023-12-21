using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Web.Model.Request;

public class AddItemToShoppingListRequest
{
    [Required]
    public required string Description { get; set; }
}