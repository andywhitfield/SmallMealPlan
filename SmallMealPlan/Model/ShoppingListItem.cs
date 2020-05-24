using System;
using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model
{
    public class ShoppingListItem
    {
        public int ShoppingListItemId { get; set; }
        [Required]
        public UserAccount User { get; set; }
        [Required]
        public Ingredient Ingredient { get; set; }
        public int SortOrder { get; set; }
        public DateTime? BoughtDateTime { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdateDateTime { get; set; }
        public DateTime? DeletedDateTime { get; set; }
    }
}