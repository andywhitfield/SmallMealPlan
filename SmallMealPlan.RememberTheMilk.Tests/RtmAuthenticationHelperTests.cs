using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SmallMealPlan.RememberTheMilk.Tests
{
    [TestClass]
    public class RtmAuthenticationHelperTests
    {
        [TestMethod]
        public void BuildAuthenticationUri()
        {
            var config = new RtmConfig("rtm-key", "rtm-secret");
            var uri = RtmAuthenticationHelper.BuildAuthenticationUri(config, RtmPermission.Delete);
            uri.Scheme.Should().Be("https");
            uri.Host.Should().Be("www.rememberthemilk.com");
            uri.AbsolutePath.Should().Be("/services/auth/");
            uri.Query.Should().Contain("perms=delete");
            uri.Query.Should().Contain("api_key=rtm-key");
            uri.Query.Should().Contain("api_sig=");
            uri.Query.Should().Contain("format=json");
        }
    }
}