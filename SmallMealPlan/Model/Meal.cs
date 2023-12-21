using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model;

public class Meal
{
    public int MealId { get; set; }
    [Required]
    public required UserAccount User { get; set; }
    public int UserAccountId { get; set; }
    [Required]
    public required string Description { get; set; }
    public List<MealIngredient> Ingredients { get; set; } = [];
    public string? Notes { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}