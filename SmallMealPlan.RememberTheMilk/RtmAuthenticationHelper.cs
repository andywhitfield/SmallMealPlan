using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;

namespace SmallMealPlan.RememberTheMilk
{
    public static class RtmAuthenticationHelper
    {
        public static Uri BuildAuthenticationUri(RtmConfig config, RtmPermission perms) =>
            new Uri(QueryHelpers.AddQueryString(
                config.AuthenticationUri.AbsoluteUri,
                new Dictionary<string, string> { { "perms", perms.ToString().ToLowerInvariant() } }.AddStandardParameters(config)
            ));
    }
}