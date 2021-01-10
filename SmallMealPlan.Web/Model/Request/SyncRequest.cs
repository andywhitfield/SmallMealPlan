using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Web.Model.Request
{
    public class SyncRequest
    {
        [Required]
        public string List { get; set; }
        public bool? Import { get; set; }
        public bool? Export { get; set; }
    }
}