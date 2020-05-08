using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model
{
    public class Meal
    {
        public int MealId { get; set; }
        [Required]
        public string Description { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        public string Notes { get; set; }
    }
}