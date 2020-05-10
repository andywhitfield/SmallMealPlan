using System;
using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model
{
    public class MealIngredient
    {
        public int MealIngredientId { get; set; }
        [Required]
        public Meal Meal { get; set; }
        [Required]
        public Ingredient Ingredient { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdateDateTime { get; set; }
    }
}