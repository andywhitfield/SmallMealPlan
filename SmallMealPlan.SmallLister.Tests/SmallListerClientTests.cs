using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace SmallMealPlan.SmallLister.Tests;

public class SmallListerClientTests
{
    [Fact]
    public async Task GetLists()
    {
        var expectedListResponse = @"{
""lists"":
[
    {
        ""listId"": ""101"",
        ""name"": ""Inbox""
    },
    {
        ""listId"": ""102"",
        ""name"": ""My List""
    }
]}";

        var mockTokenResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{""accessToken"": ""new-access-token""}") };
        var mockListResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedListResponse) };
        var mockHandler = new Mock<HttpClientHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath == "/api/v1/token"), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(mockTokenResponse));
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath == "/api/v1/list"), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(mockListResponse));

        var clientFactoryMock = new Mock<IHttpClientFactory>();
        clientFactoryMock.Setup(x => x.CreateClient(SmallListerClient.HttpClientName)).Returns(new HttpClient(mockHandler.Object));
        var config = new SmallListerConfig(new Uri("http://server"), "api-key", "app-secret");
        var client = new SmallListerClient(Mock.Of<ILogger<SmallListerClient>>(), clientFactoryMock.Object, config);
        var listsResponse = await client.GetListsAsync("refresh-token");
        listsResponse.Should().NotBeNull();
        listsResponse.Should().HaveCount(2);

        listsResponse.ElementAt(0).ListId.Should().Be("101");
        listsResponse.ElementAt(0).Name.Should().Be("Inbox");

        listsResponse.ElementAt(1).ListId.Should().Be("102");
        listsResponse.ElementAt(1).Name.Should().Be("My List");
    }

    [Fact]
    public async Task GetList()
    {
        var expectedListResponse = @"{
""listId"": ""102"",
""name"": ""My List"",
""items"":
[
    {
        ""listId"": ""102"",
        ""itemId"": ""1001"",
        ""description"": ""item 1"",
        ""dueDate"": null,
        ""notes"": null
    },
    {
        ""listId"": ""102"",
        ""itemId"": ""1002"",
        ""description"": ""item 2"",
        ""dueDate"": ""2021-01-13T00:00:00.0000000Z"",
        ""notes"": ""notes 2""
    }
]}";

        var mockTokenResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{""accessToken"": ""new-access-token""}") };
        var mockListResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedListResponse) };
        var mockHandler = new Mock<HttpClientHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath == "/api/v1/token"), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(mockTokenResponse));
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath == "/api/v1/list/102"), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(mockListResponse));

        var clientFactoryMock = new Mock<IHttpClientFactory>();
        clientFactoryMock.Setup(x => x.CreateClient(SmallListerClient.HttpClientName)).Returns(new HttpClient(mockHandler.Object));
        var config = new SmallListerConfig(new Uri("http://server"), "api-key", "app-secret");
        var client = new SmallListerClient(Mock.Of<ILogger<SmallListerClient>>(), clientFactoryMock.Object, config);
        var listResponse = await client.GetListAsync("refresh-token", "102");
        listResponse.Should().NotBeNull();
        listResponse.Name.Should().Be("My List");
        listResponse.Items.Should().HaveCount(2);

        listResponse.Items.ElementAt(0).Description.Should().Be("item 1");
        listResponse.Items.ElementAt(0).DueDate.Should().BeNull();
        listResponse.Items.ElementAt(0).ItemId.Should().Be("1001");
        listResponse.Items.ElementAt(0).ListId.Should().Be("102");
        listResponse.Items.ElementAt(0).Notes.Should().BeNull();

        listResponse.Items.ElementAt(1).Description.Should().Be("item 2");
        listResponse.Items.ElementAt(1).DueDate.Should().Be(DateTime.ParseExact("2021-01-13T00:00:00.0000000Z", "o", null, System.Globalization.DateTimeStyles.RoundtripKind));
        listResponse.Items.ElementAt(1).ItemId.Should().Be("1002");
        listResponse.Items.ElementAt(1).ListId.Should().Be("102");
        listResponse.Items.ElementAt(1).Notes.Should().Be("notes 2");
    }

    [Fact]
    public async Task RegisterForWebhooks()
    {
        List<HttpRequestMessage> webhookRequests = new();
        Mock<HttpClientHandler> mockHandler = new();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath == "/api/v1/token"), ItExpr.IsAny<CancellationToken>())
            .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{""accessToken"": ""new-access-token""}") }));
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath == "/api/v1/webhook"), ItExpr.IsAny<CancellationToken>())
            .Callback((HttpRequestMessage r, CancellationToken t) => webhookRequests.Add(r))
            .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath == "/api/v1/webhook/ListChange" && m.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>())
            .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath == "/api/v1/webhook/ListItemChange" && m.Method == HttpMethod.Delete), ItExpr.IsAny<CancellationToken>())
            .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        Mock<IHttpClientFactory> clientFactoryMock = new();
        clientFactoryMock.Setup(x => x.CreateClient(SmallListerClient.HttpClientName)).Returns(() => new HttpClient(mockHandler.Object));
        SmallListerConfig config = new(new("http://server"), "api-key", "app-secret");
        SmallListerClient client = new(Mock.Of<ILogger<SmallListerClient>>(), clientFactoryMock.Object, config);
        await client.RegisterWebhookAsync("refreshToken", new("http://client"), "userId");

        webhookRequests.Should().HaveCount(2);
        var request = await webhookRequests[0].Content.ReadAsStringAsync();
        request.Should().Be("""{"Webhook":"http://client/api/webhook/userId/smalllister/list","WebhookType":"ListChange"}""");
        request = await webhookRequests[1].Content.ReadAsStringAsync();
        request.Should().Be("""{"Webhook":"http://client/api/webhook/userId/smalllister/listitem","WebhookType":"ListItemChange"}""");
    }
}