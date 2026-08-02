using Microsoft.EntityFrameworkCore;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public class NoteHistoryRepository(SqliteDataContext context)
    : INoteHistoryRepository
{
    public async Task<NoteHistory?> GetAsync(int noteHistoryId)
        => await context.NoteHistories.Include(nh => nh.Note).SingleOrDefaultAsync(nh => nh.NoteHistoryId == noteHistoryId);

    public IAsyncEnumerable<NoteHistory> GetByNoteIdAsync(int noteId)
        => context.NoteHistories.Where(nh => nh.NoteId == noteId).AsAsyncEnumerable();
}
