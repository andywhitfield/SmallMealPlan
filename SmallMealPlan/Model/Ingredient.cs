using System;
using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public UserAccount CreatedBy { get; set; }
        public DateTime? LastUpdateDateTime { get; set; }
        public UserAccount LastUpdatedBy { get; set; }
    }
}