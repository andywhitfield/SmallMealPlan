using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public interface INoteRepository
{
    Task<Note?> GetAsync(int noteId);
    IAsyncEnumerable<Note> GetAllAsync(UserAccount user);

    Task AddOrUpdateAsync(UserAccount user, string noteText);
}