using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SmallMealPlan.RememberTheMilk;

internal static class RtmSigning
{
    public static IDictionary<string, string?> AddStandardParameters(this IDictionary<string, string?> queryParams, RtmConfig config)
    {
        queryParams["api_key"] = config.ApiKey;
        queryParams["format"] = "json";

        var queryToSign = new StringBuilder(config.SharedSecret);
        foreach (var kv in queryParams.OrderBy(kvs => kvs.Key))
            queryToSign.Append(kv.Key).Append(kv.Value);
        using var md5 = MD5.Create();
        var sig = md5.ComputeHash(Encoding.UTF8.GetBytes(queryToSign.ToString()));
        queryToSign.Clear();
        for (int i = 0; i < sig.Length; i++)
            queryToSign.Append(sig[i].ToString("x2"));
        queryParams["api_sig"] = queryToSign.ToString();
        return queryParams;
    }
}