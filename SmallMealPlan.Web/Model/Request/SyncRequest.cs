using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Web.Model.Request;

public class SyncRequest
{
    [Required]
    public required string List { get; set; }
    public bool? Sync { get; set; }
    public bool? Import { get; set; }
    public bool? Export { get; set; }
    public bool? DeleteAfterExport { get; set; }
}