using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        [Required]
        public string Description { get; set; }
    }
}