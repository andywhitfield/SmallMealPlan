using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SmallMealPlan.RememberTheMilk.Contracts;

namespace SmallMealPlan.RememberTheMilk
{
    public class RtmClient : IRtmClient
    {
        public const string HttpClientName = nameof(RtmClient);
        private readonly ILogger<RtmClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RtmConfig _config;

        public RtmClient(ILogger<RtmClient> logger, IHttpClientFactory httpClientFactory, RtmConfig config)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<RtmAuthGetTokenResponse> GetTokenAsync(string frob)
        {
            var queryParams = new Dictionary<string, string>{
                {"method", "rtm.auth.getToken"},
                {"frob", frob}
            };
            var requestUri = new Uri(QueryHelpers.AddQueryString(_config.EndpointUri.AbsoluteUri, queryParams.AddStandardParameters(_config)));
            _logger.LogTrace($"Calling RTM api: {requestUri}");

            using var httpClient = _httpClientFactory.CreateClient(HttpClientName);
            var response = await httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error calling RTM: {response.StatusCode}: {(await response.Content.ReadAsStringAsync())}");
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogTrace($"Received RTM response from [{requestUri}]: {responseString}");

            var responseObj = JsonSerializer.Deserialize<RtmJsonResponse<RtmAuthGetTokenResponse>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (!responseObj.IsSuccess)
                throw new ApplicationException($"Error getting token from frob: {responseString}");
            return responseObj.Rsp;
        }

        public async Task<RtmListsGetListResponse> GetListsAsync(string authToken)
        {
            var queryParams = new Dictionary<string, string>{
                {"method", "rtm.lists.getList"},
                {"auth_token", authToken}
            };
            var requestUri = new Uri(QueryHelpers.AddQueryString(_config.EndpointUri.AbsoluteUri, queryParams.AddStandardParameters(_config)));
            _logger.LogTrace($"Calling RTM api: {requestUri}");

            using var httpClient = _httpClientFactory.CreateClient(HttpClientName);
            var response = await httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error calling RTM: {response.StatusCode}: {(await response.Content.ReadAsStringAsync())}");
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogTrace($"Received RTM response from [{requestUri}]: {responseString}");

            var responseObj = JsonSerializer.Deserialize<RtmJsonResponse<RtmListsGetListResponse>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (!responseObj.IsSuccess)
                throw new ApplicationException($"Error getting lists: {responseString}");
            return responseObj.Rsp;
        }
    }
}