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

    public async Task AddOrUpdateAsync(UserAccount user, string noteText)
    {
        var note = await context.Notes.FirstOrDefaultAsync(n => n.User == user);
        if (note == null)
        {
            note = new Note
            {
                User = user,
                NoteText = noteText
            };
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug($"Creating new note for user: {user}: {noteText}");
            await context.Notes.AddAsync(note);
        }
        else
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug($"Updating note for user: {user}: {noteText}");
            note.NoteText = noteText;
            note.LastUpdateDateTime = DateTime.UtcNow;
        }
        await context.SaveChangesAsync();
    }
}