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

    private async Task AddNotesAsync()
    {
        await _webApplicationFactory.CreateTestUserAsync();
        await using var services = _webApplicationFactory.Services.CreateAsyncScope();
        var context = services.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var user1 = await context.UserAccounts.SingleAsync();
        var user2 = context.UserAccounts.Add(new() { Email = "test-user-2" });
        context.Notes.Add(new() { User = user1, NoteText = "note 1", CreatedDateTime = DateTime.UtcNow, SortOrdering = 0 });
        context.Notes.Add(new() { User = user1, NoteText = "note 2 text", Title = "note 2 title", CreatedDateTime = DateTime.UtcNow.AddMinutes(5), SortOrdering = 1 });
        context.Notes.Add(new() { User = user1, NoteText = "note 3", CreatedDateTime = DateTime.UtcNow.AddMinutes(3), SortOrdering = 2 });
        context.Notes.Add(new() { User = user1, NoteText = "note 4", CreatedDateTime = DateTime.UtcNow.AddMinutes(2), LastUpdateDateTime = DateTime.UtcNow.AddMinutes(4), SortOrdering = 3 });
        context.Notes.Add(new() { User = user1, NoteText = "note 5", CreatedDateTime = DateTime.UtcNow.AddMinutes(1), DeletedDateTime = DateTime.UtcNow });
        
        context.Notes.Add(new() { User = user2.Entity, NoteText = "other user's note" });
        await context.SaveChangesAsync();
    }
}