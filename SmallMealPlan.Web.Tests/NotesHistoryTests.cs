using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallMealPlan.Data;

namespace SmallMealPlan.Web.Tests;

[TestClass]
public class NotesHistoryTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();

    [TestMethod]
    public async Task Should_show_all_history_for_note()
    {
        var noteId = await AddNoteHistoryAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.GetAsync($"/notes/history/{noteId}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();

        // check the existance and order of the notes - the order should be the most recent update first
        // so should be note 1 title v4, then note 1 title v3, etc.
        var idx = responseContent.IndexOf("note 1 title v4");
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("note 1 title v3", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("note 1 title v2", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("note 1 title v1", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
    }

    [TestMethod]
    public async Task Should_get_details_for_note_history()
    {
        var noteId = await AddNoteHistoryAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.GetAsync($"/api/notehistory/{await GetNoteHistoryIdAsync(noteId, 3)}/info/details");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<NoteHistoryInfo>();
        Assert.AreEqual("note 1 title v2", responseContent?.title);
        Assert.AreEqual("note 1 v2", responseContent?.note);
    }

    [TestMethod]
    public async Task Should_get_summary_for_note_history()
    {
        var noteId = await AddNoteHistoryAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.GetAsync($"/api/notehistory/{await GetNoteHistoryIdAsync(noteId, 2)}/info/summary");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadFromJsonAsync<NoteHistoryInfo>();
        Assert.AreEqual("note 1 title v1", responseContent?.title);
        Assert.IsNull(responseContent?.note);
    }

    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    private record NoteHistoryInfo(string title, string note);

    private async Task<int> AddNoteHistoryAsync()
    {
        await _webApplicationFactory.CreateTestUserAsync();
        await using var services = _webApplicationFactory.Services.CreateAsyncScope();
        var context = services.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var user1 = await context.UserAccounts.SingleAsync();

        var time = DateTime.UtcNow;
        var newNote = context.Notes.Add(new() { User = user1, Title = "note 1 title v4", NoteText = "note 1 v4", CreatedDateTime = time.AddHours(-5), LastUpdateDateTime = time });
        context.NoteHistories.Add(new() { Note = newNote.Entity, Title = "note 1 title v1", NoteText = "note 1 v1", AuditDateTime = time.AddHours(-2) });
        context.NoteHistories.Add(new() { Note = newNote.Entity, Title = "note 1 title v2", NoteText = "note 1 v2", AuditDateTime = time.AddHours(-3) });
        context.NoteHistories.Add(new() { Note = newNote.Entity, Title = "note 1 title v3", NoteText = "note 1 v3", AuditDateTime = time.AddHours(-4) });

        await context.SaveChangesAsync();
        return newNote.Entity.NoteId;
    }

    private async Task<int> GetNoteHistoryIdAsync(int noteId, int auditDateHoursAgo)
    {
        await using var services = _webApplicationFactory.Services.CreateAsyncScope();
        var context = services.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var expectedAuditDateTime = (await context.Notes.FindAsync(noteId))?.LastUpdateDateTime?.AddHours(-auditDateHoursAgo);
        Assert.IsNotNull(expectedAuditDateTime);
        return (await context.NoteHistories.SingleAsync(nh => nh.NoteId == noteId && nh.AuditDateTime == expectedAuditDateTime)).NoteHistoryId;
    }
}