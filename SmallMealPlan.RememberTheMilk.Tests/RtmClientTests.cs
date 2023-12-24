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

namespace SmallMealPlan.RememberTheMilk.Tests;

public class RtmClientTests
{
    [Fact]
    public async Task CreateFromFrob()
    {
        var expectedResponse = @"{
""rsp"":{
    ""stat"":""ok"",
    ""auth"":{
        ""token"":""6410bde19b6dfb474fec71f186bc715831ea6842"",
        ""perms"":""write"",
        ""user"":{
            ""id"":""123432"",
            ""username"":""johnsmith"",
            ""fullname"":""John Smith""
        }
    }
}}";

        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedResponse) };
        var mockHandler = new Mock<HttpClientHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(mockResponse));

        var clientFactoryMock = new Mock<IHttpClientFactory>();
        clientFactoryMock.Setup(x => x.CreateClient(RtmClient.HttpClientName)).Returns(new HttpClient(mockHandler.Object));
        var config = new RtmConfig("api-key", "shared-secret");
        var client = new RtmClient(Mock.Of<ILogger<RtmClient>>(), clientFactoryMock.Object, config);
        var tokenResponse = await client.GetTokenAsync("frob");
        tokenResponse.Token.Should().Be("6410bde19b6dfb474fec71f186bc715831ea6842");
        tokenResponse.Perms.Should().BeEquivalentTo(RtmPermission.Write.ToString());
        tokenResponse.User.Should().NotBeNull();
        tokenResponse.User?.Id.Should().Be("123432");
        tokenResponse.User?.Username.Should().Be("johnsmith");
        tokenResponse.User?.Fullname.Should().Be("John Smith");
    }

    [Fact]
    public async Task GetLists()
    {
        var expectedResponse = @"{
""rsp"": {
    ""stat"": ""ok"",
    ""lists"": {
        ""list"": [
            {
                ""id"": ""101"",
                ""name"": ""Inbox"",
                ""deleted"": ""0"",
                ""locked"": ""1"",
                ""archived"": ""0"",
                ""position"": ""-1"",
                ""smart"": ""0"",
                ""sort_order"": ""0""
            },
            {
                ""id"": ""102"",
                ""name"": ""My List"",
                ""deleted"": ""0"",
                ""locked"": ""1"",
                ""archived"": ""0"",
                ""position"": ""1"",
                ""smart"": ""0"",
                ""sort_order"": ""0""
            }
        ]
    }
}}";

        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedResponse) };
        var mockHandler = new Mock<HttpClientHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(mockResponse));

        var clientFactoryMock = new Mock<IHttpClientFactory>();
        clientFactoryMock.Setup(x => x.CreateClient(RtmClient.HttpClientName)).Returns(new HttpClient(mockHandler.Object));
        var config = new RtmConfig("api-key", "shared-secret");
        var client = new RtmClient(Mock.Of<ILogger<RtmClient>>(), clientFactoryMock.Object, config);
        var listsResponse = await client.GetListsAsync("auth-token");
        listsResponse.List.Should().NotBeNull();
        listsResponse.List.Should().HaveCount(2);

        listsResponse.List?.ElementAt(0).Id.Should().Be("101");
        listsResponse.List?.ElementAt(0).Name.Should().Be("Inbox");
        listsResponse.List?.ElementAt(0).Deleted.Should().Be("0");
        listsResponse.List?.ElementAt(0).Locked.Should().Be("1");
        listsResponse.List?.ElementAt(0).Archived.Should().Be("0");
        listsResponse.List?.ElementAt(0).Position.Should().Be("-1");
        listsResponse.List?.ElementAt(0).Smart.Should().Be("0");

        listsResponse.List?.ElementAt(1).Id.Should().Be("102");
        listsResponse.List?.ElementAt(1).Name.Should().Be("My List");
        listsResponse.List?.ElementAt(1).Deleted.Should().Be("0");
        listsResponse.List?.ElementAt(1).Locked.Should().Be("1");
        listsResponse.List?.ElementAt(1).Archived.Should().Be("0");
        listsResponse.List?.ElementAt(1).Position.Should().Be("1");
        listsResponse.List?.ElementAt(1).Smart.Should().Be("0");
    }

    [Fact]
    public async Task GetTasks()
    {
        var expectedResponse = @"{
""rsp"": {
    ""stat"": ""ok"",
    ""tasks"": {
        ""list"": [
            {
                ""id"": ""1000"",
                ""taskseries"": [
                    {
                        ""id"": ""1001"",
                        ""created"": ""2020-06-20T15:32:00Z"",
                        ""modified"": ""2020-06-20T15:32:00Z"",
                        ""name"": ""item 1""
                    },
                    {
                        ""id"": ""1002"",
                        ""created"": ""2020-06-20T15:33:00Z"",
                        ""modified"": ""2020-06-20T15:33:00Z"",
                        ""name"": ""item 2""
                    }
                ]
            }
        ]
    }
}}";

        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedResponse) };
        var mockHandler = new Mock<HttpClientHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(mockResponse));

        var clientFactoryMock = new Mock<IHttpClientFactory>();
        clientFactoryMock.Setup(x => x.CreateClient(RtmClient.HttpClientName)).Returns(new HttpClient(mockHandler.Object));
        var config = new RtmConfig("api-key", "shared-secret");
        var client = new RtmClient(Mock.Of<ILogger<RtmClient>>(), clientFactoryMock.Object, config);
        var tasksResponse = await client.GetTaskListsAsync("auth-token", "1000");
        tasksResponse.List.Should().NotBeNull();
        tasksResponse.List.Should().HaveCount(1);

        tasksResponse.List.ElementAt(0).Id.Should().Be("1000");
        tasksResponse.List.ElementAt(0).TaskSeries.Should().NotBeNull();
        tasksResponse.List.ElementAt(0).TaskSeries.Should().HaveCount(2);
        tasksResponse.List.ElementAt(0).TaskSeries.ElementAt(0).Id.Should().Be("1001");
        tasksResponse.List.ElementAt(0).TaskSeries.ElementAt(0).Name.Should().Be("item 1");
        tasksResponse.List.ElementAt(0).TaskSeries.ElementAt(1).Id.Should().Be("1002");
        tasksResponse.List.ElementAt(0).TaskSeries.ElementAt(1).Name.Should().Be("item 2");
    }

    [Fact]
    public async Task AddTask()
    {
        var expectedResponse = @"{
""rsp"": {
    ""stat"": ""ok"",
    ""list"": {
        ""id"": ""3000"",
        ""taskseries"": [
            {
                ""id"": ""3001"",
                ""created"": ""2020-06-20T15:34:00Z"",
                ""modified"": ""2020-06-20T15:34:00Z"",
                ""name"": ""new item 3""
            }
        ]
    }
}}";

        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedResponse) };
        var mockHandler = new Mock<HttpClientHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(mockResponse));

        var clientFactoryMock = new Mock<IHttpClientFactory>();
        clientFactoryMock.Setup(x => x.CreateClient(RtmClient.HttpClientName)).Returns(new HttpClient(mockHandler.Object));
        var config = new RtmConfig("api-key", "shared-secret");
        var client = new RtmClient(Mock.Of<ILogger<RtmClient>>(), clientFactoryMock.Object, config);
        var addResponse = await client.AddTaskAsync("auth-token", "timeline-1", "2000", "new item 3");
        addResponse.Should().NotBeNull();
        addResponse.Id.Should().Be("3000");
        addResponse.TaskSeries.Should().HaveCount(1);
        addResponse.TaskSeries.ElementAt(0).Should().NotBeNull();
        addResponse.TaskSeries.ElementAt(0).Id.Should().Be("3001");
        addResponse.TaskSeries.ElementAt(0).Name.Should().Be("new item 3");
    }

    [Fact]
    public async Task CreateTimeline()
    {
        var expectedResponse = @"{
""rsp"": {
    ""stat"": ""ok"",
    ""timeline"": ""new-timeline""
}}";

        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(expectedResponse) };
        var mockHandler = new Mock<HttpClientHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(Task.FromResult(mockResponse));

        var clientFactoryMock = new Mock<IHttpClientFactory>();
        clientFactoryMock.Setup(x => x.CreateClient(RtmClient.HttpClientName)).Returns(new HttpClient(mockHandler.Object));
        var config = new RtmConfig("api-key", "shared-secret");
        var client = new RtmClient(Mock.Of<ILogger<RtmClient>>(), clientFactoryMock.Object, config);
        var timelineResponse = await client.CreateTimelineAsync("auth-token");
        timelineResponse.Should().Be("new-timeline");
    }
}