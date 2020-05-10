using System;
using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model
{
    public class PlannerMeal
    {
        public int PlannerMealId { get; set; }
        public DateTime Date { get; set; }
        [Required]
        public Meal Meal { get; set; }
        [Required]
        public UserAccount User { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdateDateTime { get; set; }
        public DateTime? DeletedDateTime { get; set; }
    }
}