using System;
using System.ComponentModel.DataAnnotations;

namespace SmallMealPlan.Model
{
    public class Note
    {
        public int NoteId { get; set; }
        [Required]
        public UserAccount User { get; set; }
        public int UserAccountId { get; set; }
        [Required]
        public string NoteText { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdateDateTime { get; set; }
    }
}