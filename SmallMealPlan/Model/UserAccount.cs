using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model
{
    public class UserAccount
    {
        public int UserAccountId { get; set; }
        [Required]
        public string AuthenticationUri { get; set; }
        public List<PlannerMeal> PlannerMeals { get; set; }
        public List<Meal> Meals { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdateDateTime { get; set; }
        public DateTime? DeletedDateTime { get; set; }
    }
}
