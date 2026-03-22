using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model;

public class NoteHistory
{
    public int NoteHistoryId { get; set; }
    [Required]
    public required Note Note { get; set; }
    public int NoteId { get; set; }
    public string? Title { get; set; }
    [Required]
    public required string NoteText { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime AuditDateTime { get; set; } = DateTime.UtcNow;
}
