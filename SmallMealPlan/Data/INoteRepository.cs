using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public interface INoteRepository
{
    public const string SortedManually = "manual";
    
    Task<Note?> GetAsync(int noteId);
    IAsyncEnumerable<Note> GetAllAsync(UserAccount user);
    Task AddAsync(UserAccount user, string? title, string noteText);
    Task UpdateAsync(Note note, string? title, string noteText);
    Task DeleteAsync(Note note);
    Task ReorderAsync(UserAccount user, int noteId, int? sortOrderPreviousNoteId);
}