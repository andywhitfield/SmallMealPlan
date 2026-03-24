using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model;

public class Note
{
    public int NoteId { get; set; }
    [Required]
    public required UserAccount User { get; set; }
    public int UserAccountId { get; set; }
    public int? SortOrdering { get; set; }
    public string? Title { get; set; }
    [Required]
    public required string NoteText { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}