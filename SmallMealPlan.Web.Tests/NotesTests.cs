using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallMealPlan.Data;

namespace SmallMealPlan.Web.Tests;

[TestClass]
public class NotesTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();

    [TestMethod]
    public async Task Given_no_credentials_should_redirect_to_login()
    {
        using var client = await _webApplicationFactory.CreateUnauthenticatedClientAsync();
        using var response = await client.GetAsync("/notes");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Should_show_all_notes()
    {
        await AddNotesAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.GetAsync("/notes");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();

        // check the existance and order of the notes - by default, the order should be the most recent first
        // so should be 2, 4, 3, 1
        var idx = responseContent.IndexOf("note 2 title");
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("note 4", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("note 3", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("note 1", idx);
        Assert.IsGreaterThan(0, idx, responseContent);

        Assert.DoesNotContain("note 2 text", responseContent);
        Assert.DoesNotContain("second line of text", responseContent);
        Assert.DoesNotContain("note 5", responseContent);
        Assert.DoesNotContain("other user's note", responseContent);
    }

    [TestMethod]
    public async Task Given_manual_ordering_Should_show_notes_in_specified_order()
    {
        await AddNotesAsync();
        await UpdateUserAccountAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.GetAsync("/notes");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();

        var idx = responseContent.IndexOf("note 1");
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("note 2 title", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("note 3", idx);
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("note 4", idx);
        Assert.IsGreaterThan(0, idx, responseContent);

        async Task UpdateUserAccountAsync()
        {
            await using var services = _webApplicationFactory.Services.CreateAsyncScope();
            var context = services.ServiceProvider.GetRequiredService<SqliteDataContext>();
            var user = await context.UserAccounts.SingleAsync(ua => ua.Email == "test-user-1");
            user.NoteSortOrdering = INoteRepository.SortedManually;
            await context.SaveChangesAsync();
        }
    }

    [TestMethod]
    public async Task Should_find_notes()
    {
        await AddNotesAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.GetAsync("/notes?find=text");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();

        // check the existance and order of the notes - by default, the order should be the most recent first
        // so should be 2, 4, 3, 1
        var idx = responseContent.IndexOf("note 2 title");
        Assert.IsGreaterThan(0, idx, responseContent);
        idx = responseContent.IndexOf("note 3", idx);
        Assert.IsGreaterThan(0, idx, responseContent);

        Assert.DoesNotContain("note 1", responseContent);
        Assert.DoesNotContain("note 2 text", responseContent);
        Assert.DoesNotContain("note 4", responseContent);
        Assert.DoesNotContain("note 5", responseContent);
        Assert.DoesNotContain("other user's note", responseContent);
    }

    [TestMethod]
    public async Task Can_add_new_note()
    {
        await AddNotesAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var getResponse = await client.GetAsync("/notes/add");
        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        var validationToken = WebApplicationFactoryTest.GetFormValidationToken(getResponseContent, "/notes/add");

        using var postResponse = await client.PostAsync("/notes/add", new FormUrlEncodedContent(new Dictionary<string, string>()
            { { "notes", "New note with only note text" }, { "__RequestVerificationToken", validationToken } }));
        Assert.AreEqual(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.AreEqual(new Uri("/notes", UriKind.Relative), postResponse.Headers.Location);

        await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
        var db = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var newNote = await db.Notes.SingleOrDefaultAsync(n => n.NoteText == "New note with only note text");
        Assert.IsNotNull(newNote);
        Assert.IsNull(newNote.Title);
    }

    [TestMethod]
    public async Task Can_add_new_note_with_title()
    {
        await AddNotesAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var getResponse = await client.GetAsync("/notes/add");
        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        var validationToken = WebApplicationFactoryTest.GetFormValidationToken(getResponseContent, "/notes/add");

        using var postResponse = await client.PostAsync("/notes/add", new FormUrlEncodedContent(new Dictionary<string, string>()
            { { "notes", "New note with a title" }, { "title", "New note title" }, { "__RequestVerificationToken", validationToken } }));
        Assert.AreEqual(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.AreEqual(new Uri("/notes", UriKind.Relative), postResponse.Headers.Location);

        await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
        var db = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var newNote = await db.Notes.SingleOrDefaultAsync(n => n.NoteText == "New note with a title");
        Assert.IsNotNull(newNote);
        Assert.AreEqual("New note title", newNote.Title);
    }

    [TestMethod]
    public async Task Should_not_add_note_without_content()
    {
        await AddNotesAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var getResponse = await client.GetAsync("/notes/add");
        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        var validationToken = WebApplicationFactoryTest.GetFormValidationToken(getResponseContent, "/notes/add");

        using var postResponse = await client.PostAsync("/notes/add", new FormUrlEncodedContent(new Dictionary<string, string>()
            { { "notes", "" }, { "title", "Note without content" }, { "__RequestVerificationToken", validationToken } }));
        Assert.AreEqual(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.AreEqual(new Uri("/notes/add", UriKind.Relative), postResponse.Headers.Location);

        await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
        var db = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        Assert.IsFalse(await db.Notes.AnyAsync(n => n.Title == "Note without content"));
    }

    [TestMethod]
    public async Task Can_update_note()
    {
        await AddNotesAsync();
        var noteId = await GetTestNoteIdAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var getResponse = await client.GetAsync($"/notes/{noteId}");
        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        var validationToken = WebApplicationFactoryTest.GetFormValidationToken(getResponseContent, $"/notes/{noteId}");

        using var postResponse = await client.PostAsync($"/notes/{noteId}", new FormUrlEncodedContent(new Dictionary<string, string>()
            { { "notes", "note 1 updated" }, { "title", "new title" }, { "__RequestVerificationToken", validationToken } }));
        Assert.AreEqual(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.AreEqual(new Uri("/notes", UriKind.Relative), postResponse.Headers.Location);

        await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
        var db = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var updatedNote = await db.Notes.FindAsync(noteId);
        Assert.IsNotNull(updatedNote);
        Assert.AreEqual("note 1 updated", updatedNote.NoteText);
        Assert.AreEqual("new title", updatedNote.Title);
        Assert.IsFalse(await db.Notes.AnyAsync(n => n.NoteText == "note 1"));

        var auditNote = await db.NoteHistories.SingleOrDefaultAsync(n => n.NoteId == noteId);
        Assert.IsNotNull(auditNote);
        Assert.AreEqual("note 1", auditNote.NoteText);
        Assert.IsNull(auditNote.Title);

        async Task<int> GetTestNoteIdAsync()
        {
            await using var services = _webApplicationFactory.Services.CreateAsyncScope();
            var context = services.ServiceProvider.GetRequiredService<SqliteDataContext>();
            return (await context.Notes.SingleAsync(n => n.NoteText == "note 1")).NoteId;
        }
    }

    [TestMethod]
    public async Task Can_delete_note()
    {
        await AddNotesAsync();
        var noteId = await GetTestNoteIdAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var getResponse = await client.GetAsync("/notes/");
        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        var validationToken = WebApplicationFactoryTest.GetFormValidationToken(getResponseContent, $"/notes/delete/{noteId}");

        using var postResponse = await client.PostAsync($"/notes/delete/{noteId}", new FormUrlEncodedContent(new Dictionary<string, string>()
            {{ "__RequestVerificationToken", validationToken }}));
        Assert.AreEqual(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.AreEqual(new Uri("/notes", UriKind.Relative), postResponse.Headers.Location);

        await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
        var db = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var deletedNote = await db.Notes.FindAsync(noteId);
        Assert.IsNotNull(deletedNote);
        Assert.AreEqual("note 4", deletedNote.NoteText);
        Assert.IsNotNull(deletedNote.DeletedDateTime);

        var auditNote = await db.NoteHistories.SingleOrDefaultAsync(n => n.NoteId == noteId);
        Assert.IsNotNull(auditNote);
        Assert.AreEqual("note 4", auditNote.NoteText);
        Assert.IsNull(auditNote.Title);
        Assert.IsFalse(auditNote.IsDeleted);

        async Task<int> GetTestNoteIdAsync()
        {
            await using var services = _webApplicationFactory.Services.CreateAsyncScope();
            var context = services.ServiceProvider.GetRequiredService<SqliteDataContext>();
            return (await context.Notes.SingleAsync(n => n.NoteText == "note 4")).NoteId;
        }
    }

    [TestMethod]
    public async Task When_manually_sorted_Can_update_sort_order()
    {
        await AddNotesAsync();
        await UpdateSortOrderToManualAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var getResponse = await client.GetAsync("/notes/");
        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("data-visible=\"true\"", getResponseContent);
        var validationToken = WebApplicationFactoryTest.GetFormValidationToken(getResponseContent, "/notes/sort");

        using var postResponse = await client.PostAsync("/notes/sort", new FormUrlEncodedContent(new Dictionary<string, string>()
            {{ "__RequestVerificationToken", validationToken }}));
        Assert.AreEqual(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.AreEqual(new Uri("/notes", UriKind.Relative), postResponse.Headers.Location);

        await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
        var db = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var user = await db.UserAccounts.SingleAsync(ua => ua.Email == "test-user-1");
        Assert.IsNotNull(user);
        Assert.IsNull(user.NoteSortOrdering);

        async Task UpdateSortOrderToManualAsync()
        {
            await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
            var db = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
            var user = await db.UserAccounts.SingleAsync(ua => ua.Email == "test-user-1");
            user.NoteSortOrdering = INoteRepository.SortedManually;
            await db.SaveChangesAsync();
        }
    }

    [TestMethod]
    public async Task When_default_sort_Cannot_update_sort_order()
    {
        await AddNotesAsync();
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var getResponse = await client.GetAsync("/notes/");
        var getResponseContent = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("data-visible=\"\"", getResponseContent);
    }

    private async Task AddNotesAsync()
    {
        await _webApplicationFactory.CreateTestUserAsync();
        await using var services = _webApplicationFactory.Services.CreateAsyncScope();
        var context = services.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var user1 = await context.UserAccounts.SingleAsync();
        var user2 = context.UserAccounts.Add(new() { Email = "test-user-2" });
        context.Notes.Add(new() { User = user1, NoteText = "note 1", CreatedDateTime = DateTime.UtcNow, SortOrdering = 0 });
        context.Notes.Add(new() { User = user1, NoteText = "note 2 text", Title = "note 2 title", CreatedDateTime = DateTime.UtcNow.AddMinutes(5), SortOrdering = 1 });
        context.Notes.Add(new() { User = user1, NoteText = "note 3\nsecond line of text", CreatedDateTime = DateTime.UtcNow.AddMinutes(3), SortOrdering = 2 });
        context.Notes.Add(new() { User = user1, NoteText = "note 4", CreatedDateTime = DateTime.UtcNow.AddMinutes(2), LastUpdateDateTime = DateTime.UtcNow.AddMinutes(4), SortOrdering = 3 });
        context.Notes.Add(new() { User = user1, NoteText = "note 5", CreatedDateTime = DateTime.UtcNow.AddMinutes(1), DeletedDateTime = DateTime.UtcNow });

        context.Notes.Add(new() { User = user2.Entity, NoteText = "other user's note" });
        await context.SaveChangesAsync();
    }
}