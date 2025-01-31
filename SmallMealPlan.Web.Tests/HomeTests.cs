using System.Net;

namespace SmallMealPlan.Web.Tests;

[TestClass]
public class HomeTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();

    [TestMethod]
    public async Task Given_no_credentials_should_redirect_to_login()
    {
        using var client = await _webApplicationFactory.CreateUnauthenticatedClientAsync();
        using var response = await client.GetAsync("/");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Given_valid_credentials_should_be_logged_in()
    {
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var response = await client.GetAsync("/");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(responseContent, "Logout");
    }

    [TestMethod]
    public async Task Added_meal_should_display_on_planner()
    {
        using var client = await _webApplicationFactory.CreateAuthenticatedClientAsync();
        using var getAddNewResponse = await client.GetAsync("/planner/20250110/add");
        Assert.AreEqual(HttpStatusCode.OK, getAddNewResponse.StatusCode);
        var getAddNewResponseContent = await getAddNewResponse.Content.ReadAsStringAsync();
        var validationToken = WebApplicationFactoryTest.GetFormValidationToken(getAddNewResponseContent, "/planner/20250110/add");

        using var postAddNewResponse = await client.PostAsync("/planner/20250110/add", new FormUrlEncodedContent(new Dictionary<string, string> {
            { "__RequestVerificationToken", validationToken },
            { "description", "Test meal description" },
            { "ingredients", "Test ingredient 1\nTest ingredient 2" },
            { "notes", "Test notes" },
            { "datenotes", "Test notes for this day" }
        }));
        Assert.AreEqual(HttpStatusCode.Redirect, postAddNewResponse.StatusCode);
        Assert.AreEqual(new Uri("/planner/20250110", UriKind.Relative), postAddNewResponse.Headers.Location);

        using var getPlannerResponse = await client.GetAsync("/planner/20250110");
        Assert.AreEqual(HttpStatusCode.OK, getPlannerResponse.StatusCode);
        var getPlannerResponseContent = await getPlannerResponse.Content.ReadAsStringAsync();
        StringAssert.Contains(getPlannerResponseContent, "Test meal description");
        StringAssert.Contains(getPlannerResponseContent, "Test ingredient 1");
        StringAssert.Contains(getPlannerResponseContent, "Test ingredient 2");
        StringAssert.Contains(getPlannerResponseContent, "Test notes");
        StringAssert.Contains(getPlannerResponseContent, "Test notes for this day");
    }
}