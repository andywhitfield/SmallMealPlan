using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallMealPlan.Model;

namespace SmallMealPlan.Data
{
    public class NoteRepository : INoteRepository
    {
        private readonly SqliteDataContext _context;
        private readonly ILogger<NoteRepository> _logger;

        public NoteRepository(SqliteDataContext context, ILogger<NoteRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GetAsync(UserAccount user)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(n => n.User == user);
            return note?.NoteText ?? "";
        }

        public async Task AddOrUpdateAsync(UserAccount user, string noteText)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(n => n.User == user);
            if (note == null)
            {
                note = new Note
                {
                    User = user,
                    NoteText = noteText
                };
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"Creating new note for user: {user}: {noteText}");
                await _context.Notes.AddAsync(note);
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"Updating note for user: {user}: {noteText}");
                note.NoteText = noteText;
                note.LastUpdateDateTime = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }
    }
}