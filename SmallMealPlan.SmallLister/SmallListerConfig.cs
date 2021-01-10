using System;

namespace SmallMealPlan.SmallLister
{
    public class SmallListerConfig
    {
        public SmallListerConfig(Uri baseUri, string appKey, string appSecret)
        {
            BaseUri = baseUri;
            AppKey = appKey;
            AppSecret = appSecret;
        }

        public Uri BaseUri { get; }
        public string AppKey { get; }
        public string AppSecret { get; }
    }
}