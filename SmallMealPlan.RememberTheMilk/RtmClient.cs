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

        public async Task<RtmAuth> GetTokenAsync(string frob)
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
            if (responseObj.Rsp.Auth == null)
                throw new ApplicationException($"Error getting auth details from frob response: {responseString}");
            return responseObj.Rsp.Auth;
        }

        public async Task<RtmLists> GetListsAsync(string authToken)
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
            if (responseObj.Rsp.Lists == null)
                throw new ApplicationException($"Error getting lists from response: {responseString}");
            return responseObj.Rsp.Lists;
        }

        public async Task<RtmTasks> GetTaskListsAsync(string authToken, string listId)
        {
            var queryParams = new Dictionary<string, string>{
                {"method", "rtm.tasks.getList"},
                {"auth_token", authToken},
                {"list_id", listId},
                {"filter", "status:incomplete"}
            };
            var requestUri = new Uri(QueryHelpers.AddQueryString(_config.EndpointUri.AbsoluteUri, queryParams.AddStandardParameters(_config)));
            _logger.LogTrace($"Calling RTM api: {requestUri}");

            using var httpClient = _httpClientFactory.CreateClient(HttpClientName);
            var response = await httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error calling RTM: {response.StatusCode}: {(await response.Content.ReadAsStringAsync())}");
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogTrace($"Received RTM response from [{requestUri}]: {responseString}");

            var responseObj = JsonSerializer.Deserialize<RtmJsonResponse<RtmTasksGetListResponse>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (!responseObj.IsSuccess)
                throw new ApplicationException($"Error getting task lists: {responseString}");
            if (responseObj.Rsp.Tasks == null)
                throw new ApplicationException($"Error getting task lists from response: {responseString}");
            return responseObj.Rsp.Tasks;
        }

        public async Task<RtmList> AddTaskAsync(string authToken, string timeline, string listId, string itemToAddToList)
        {
            var queryParams = new Dictionary<string, string>{
                {"method", "rtm.tasks.add"},
                {"auth_token", authToken},
                {"timeline", timeline},
                {"list_id", listId},
                {"name", itemToAddToList}
            };
            var requestUri = new Uri(QueryHelpers.AddQueryString(_config.EndpointUri.AbsoluteUri, queryParams.AddStandardParameters(_config)));
            _logger.LogTrace($"Calling RTM api: {requestUri}");

            using var httpClient = _httpClientFactory.CreateClient(HttpClientName);
            var response = await httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error calling RTM: {response.StatusCode}: {(await response.Content.ReadAsStringAsync())}");
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.LogTrace($"Received RTM response from [{requestUri}]: {responseString}");

            var responseObj = JsonSerializer.Deserialize<RtmJsonResponse<RtmTasksAddResponse>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (!responseObj.IsSuccess)
                throw new ApplicationException($"Error adding to list: {responseString}");
            if (responseObj.Rsp.List == null)
                throw new ApplicationException($"Error getting task list from add task response: {responseString}");
            return responseObj.Rsp.List;
        }

        public async Task<string> CreateTimelineAsync(string authToken)
        {
            var queryParams = new Dictionary<string, string>{
                {"method", "rtm.timelines.create"},
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

            var responseObj = JsonSerializer.Deserialize<RtmJsonResponse<RtmTimelinesCreateResponse>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (!responseObj.IsSuccess)
                throw new ApplicationException($"Error creating timeline: {responseString}");
            return responseObj.Rsp.Timeline;
        }
    }
}