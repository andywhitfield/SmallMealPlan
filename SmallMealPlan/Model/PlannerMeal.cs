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
    }
}