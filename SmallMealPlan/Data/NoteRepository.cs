using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public class NoteRepository(SqliteDataContext context, ILogger<NoteRepository> logger)
    : INoteRepository
{
    public async Task<Note?> GetAsync(int noteId)
        => await context.Notes.SingleOrDefaultAsync(n => n.NoteId == noteId && n.DeletedDateTime == null);

    public IAsyncEnumerable<Note> GetAllAsync(UserAccount user)
        => context.Notes.Where(n => n.UserAccountId == user.UserAccountId && n.DeletedDateTime == null).AsAsyncEnumerable();

    public async Task AddAsync(UserAccount user, string? title, string noteText)
    {
        logger.LogDebug("Creating new note for user: {User}: [{Title}]: {NoteText}", user.UserAccountId, title, noteText);
        context.Notes.Add(new()
        {
            User = user,
            Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            NoteText = noteText
        });
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Note note, string? title, string noteText)
    {
        logger.LogDebug("Updating note {NoteId}: [{Title}]: {NoteText}", note.NoteId, title, noteText);
        context.NoteHistories.Add(new()
        {
            Note = note,
            Title = note.Title,
            NoteText = note.NoteText
        });
        note.Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        note.NoteText = noteText;
        note.LastUpdateDateTime = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }
}