using System.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data;

public class NoteRepository(SqliteDataContext context, ILogger<NoteRepository> logger)
    : INoteRepository
{
    public async Task<Note?> GetAsync(int noteId)
        => await context.Notes.SingleOrDefaultAsync(n => n.NoteId == noteId && n.DeletedDateTime == null);

    public IAsyncEnumerable<Note> GetAllAsync(UserAccount user, string? find)
    {
        var notes = context.Notes.Where(n => n.UserAccountId == user.UserAccountId && n.DeletedDateTime == null);
        if (user.NoteSortOrdering == INoteRepository.SortedManually)
            notes = notes.OrderBy(n => n.SortOrdering ?? 0);
        else
            notes = notes.OrderByDescending(n => n.LastUpdateDateTime ?? n.CreatedDateTime);
    
        if (!string.IsNullOrWhiteSpace(find))
        {
            var like = $"%{find.Trim()}%";
            notes = notes.Where(n => (n.Title != null && EF.Functions.Like(n.Title, like)) || EF.Functions.Like(n.NoteText, like));
        }

        return notes.AsAsyncEnumerable();
    }

    public async Task AddAsync(UserAccount user, string? title, string noteText)
    {
        logger.LogDebug("Creating new note for user: {User}: [{Title}]: {NoteText}", user.UserAccountId, title, noteText);
        context.Notes.Add(new()
        {
            User = user,
            Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            NoteText = noteText,
            SortOrdering = user.NoteSortOrdering == INoteRepository.SortedManually ? (context.Notes.Where(n => n.DeletedDateTime == null).Max(n => n.SortOrdering) ?? -1) + 1 : null
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

    public async Task DeleteAsync(Note note)
    {
        logger.LogDebug("Deleting note {NoteId}", note.NoteId);
        context.NoteHistories.Add(new()
        {
            Note = note,
            Title = note.Title,
            NoteText = note.NoteText
        });
        note.DeletedDateTime = DateTime.UtcNow;
        note.LastUpdateDateTime = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task ReorderAsync(UserAccount user, int noteId, int? sortOrderPreviousNoteId)
    {
        ArgumentNullException.ThrowIfNull(user);

        var note = await context.Notes.FindAsync(noteId);
        if (note == null)
            return;

        if (note.User.UserAccountId != user.UserAccountId)
            throw new SecurityException($"Cannot update note id: {note.NoteId}");

        logger.LogDebug("Update note id: {NoteId} to be after {SortOrderPreviousNoteId}", note.NoteId, sortOrderPreviousNoteId);

        var notes = await context.Notes
            .Where(n => n.UserAccountId == user.UserAccountId && n.DeletedDateTime == null)
            .ToListAsync();
        
        if (user.NoteSortOrdering != INoteRepository.SortedManually)
        {
            user.NoteSortOrdering = INoteRepository.SortedManually;
            var order = 0;
            foreach (var n in notes.OrderByDescending(n => n.LastUpdateDateTime ?? n.CreatedDateTime))
                n.SortOrdering = order++;
        }

        int? sortOrder = null;
        if (!sortOrderPreviousNoteId.HasValue)
        {
            sortOrder = 0;
            note.SortOrdering = sortOrder.Value;
            sortOrder++;
        }

        Note? lastNote = null;
        foreach (var n in notes)
        {
            if (n.NoteId == note.NoteId)
                continue;

            if (sortOrderPreviousNoteId == n.NoteId)
            {
                sortOrder = (n.SortOrdering ?? 0) + 1;
                note.SortOrdering = sortOrder.Value;
                sortOrder++;
            }
            else if (sortOrder.HasValue)
            {
                n.SortOrdering = sortOrder.Value;
                sortOrder++;
            }

            lastNote = n;
        }

        if (!sortOrder.HasValue)
            note.SortOrdering = lastNote?.SortOrdering ?? 0;

        await context.SaveChangesAsync();
    }
}
