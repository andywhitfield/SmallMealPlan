using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Web.Model.Request
{
    public class ShoppingListItemMoveRequest
    {
        public int? SortOrderPreviousShoppingListItemId { get; set; }
    }
}