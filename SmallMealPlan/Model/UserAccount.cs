using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model;

public class UserAccount
{
    public int UserAccountId { get; set; }
    [Required]
    public required string Email { get; set; }
    public List<PlannerMeal> PlannerMeals { get; set; } = [];
    public List<Meal> Meals { get; set; } = [];
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
    public string? RememberTheMilkToken { get; set; }
    public string? RememberTheMilkLastListId { get; set; }
    public string? SmallListerToken { get; set; }
    public string? SmallListerLastListId { get; set; }
    public string? SmallListerSyncListId { get; set; }
    public string? SmallListerSyncListName { get; set; }
}
