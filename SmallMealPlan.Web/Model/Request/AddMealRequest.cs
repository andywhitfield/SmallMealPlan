using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Web.Model.Request
{
    public class AddMealRequest
    {
        [Required]
        public string Description { get; set; }
        public string Ingredients { get; set; }
        public string Notes { get; set; }
    }
}