using System.Collections.Generic;

namespace SmallMealPlan.Web.Model.ShoppingList
{
    public class MealItemModel
    {
        public int MealId { get; set; }
        public string Description { get; set; }
        public IEnumerable<IngredientItemModel> Ingredients { get; set; } = new List<IngredientItemModel>();
    }
}