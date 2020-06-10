using System;

namespace SmallMealPlan.RememberTheMilk
{
    public class RtmConfig
    {
        public RtmConfig() { }
        public RtmConfig(string apiKey, string sharedSecret)
        {
            ApiKey = apiKey;
            SharedSecret = sharedSecret;
        }

        public string ApiKey { get; set; }
        public string SharedSecret { get; set; }
        public Uri AuthenticationUri { get; set; } = new Uri("https://www.rememberthemilk.com/services/auth/");
        public Uri EndpointUri { get; set; } = new Uri("https://api.rememberthemilk.com/services/rest/");
    }
}