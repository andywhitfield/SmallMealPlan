using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Web.Model.Request
{
    public class AddItemToShoppingListFromPlannerRequest
    {
        [Required]
        public int[] IngredientId { get; set; }
    }
}