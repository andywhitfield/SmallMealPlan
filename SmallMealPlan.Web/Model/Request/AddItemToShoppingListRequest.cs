using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Web.Model.Request
{
    public class AddItemToShoppingListRequest
    {
        [Required]
        public string Description { get; set; }
    }
}