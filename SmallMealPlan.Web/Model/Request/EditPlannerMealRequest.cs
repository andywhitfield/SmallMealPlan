using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Web.Model.Request
{
    public class EditPlannerMealRequest
    {
        [Required]
        public string Description { get; set; }
        public string Ingredients { get; set; }
        public string Notes { get; set; }
        public bool? Save { get; set; }
        public bool? SaveAsNew { get; set; }
    }
}