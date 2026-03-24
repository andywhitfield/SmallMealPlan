using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public interface INoteRepository
{
    Task<Note?> GetAsync(int noteId);
    IAsyncEnumerable<Note> GetAllAsync(UserAccount user);
    Task AddAsync(UserAccount user, string? title, string noteText);
    Task UpdateAsync(Note note, string? title, string noteText);
}