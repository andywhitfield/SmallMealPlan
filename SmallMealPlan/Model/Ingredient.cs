using System;
using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model;

public class Ingredient
{
    public int IngredientId { get; set; }
    [Required]
    public required string Description { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public UserAccount? CreatedBy { get; set; }
    public DateTime? LastUpdateDateTime { get; set; }
    public UserAccount? LastUpdatedBy { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}