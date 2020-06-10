using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace SmallMealPlan.RememberTheMilk.Tests
{
    public class RtmClientTests
    {
        [Fact]
        public async Task CreateFromFrob()
        {
            var expectedResponse = @"{
    ""rsp"":{
        ""stat"":""ok"",
        ""token"":""6410bde19b6dfb474fec71f186bc715831ea6842"",
        ""perms"":""write"",
        ""user"":{
            ""id"":""987654321"",
            ""username"":""user1"",
            ""fullname"":""test user 1""
        }
    }
}";

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
        }
    }
}