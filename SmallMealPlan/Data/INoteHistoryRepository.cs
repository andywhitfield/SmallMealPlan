using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public interface INoteHistoryRepository
{
    Task<NoteHistory?> GetAsync(int noteHistoryId);
    IAsyncEnumerable<NoteHistory> GetByNoteIdAsync(int noteId);
}