using System;
using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model;

public class MealIngredient
{
    public int MealIngredientId { get; set; }
    [Required]
    public Meal? Meal { get; set; }
    public int MealId { get; set; }
    [Required]
    public required Ingredient Ingredient { get; set; }
    public int IngredientId { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}