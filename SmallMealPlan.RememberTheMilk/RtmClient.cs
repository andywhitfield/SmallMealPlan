using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SmallMealPlan.RememberTheMilk.Contracts;

namespace SmallMealPlan.RememberTheMilk;

public class RtmClient(ILogger<RtmClient> logger, IHttpClientFactory httpClientFactory, RtmConfig config)
    : IRtmClient
{
    public const string HttpClientName = nameof(RtmClient);

    public async Task<RtmAuth> GetTokenAsync(string frob)
    {
        var tokenResponse = await CallRtmMethodAsync<RtmAuthGetTokenResponse>(new Dictionary<string, string?>{
            {"method", "rtm.auth.getToken"},
            {"frob", frob}
        });
        if (tokenResponse?.Auth == null)
            throw new ApplicationException($"Error getting auth details from frob response: {tokenResponse?.InfoString}");
        return tokenResponse.Auth;
    }

    public async Task<RtmLists> GetListsAsync(string? authToken)
    {
        var listResponse = await CallRtmMethodAsync<RtmListsGetListResponse>(new Dictionary<string, string?>{
            {"method", "rtm.lists.getList"},
            {"auth_token", authToken ?? throw new ArgumentNullException(nameof(authToken))}
        });
        if (listResponse?.Lists == null)
            throw new ApplicationException($"Error getting lists from response: {listResponse?.InfoString}");
        return listResponse.Lists;
    }

    public async Task<RtmTasks> GetTaskListsAsync(string? authToken, string listId)
    {
        var taskListResponse = await CallRtmMethodAsync<RtmTasksGetListResponse>(new Dictionary<string, string?>{
            {"method", "rtm.tasks.getList"},
            {"auth_token", authToken ?? throw new ArgumentNullException(nameof(authToken))},
            {"list_id", listId},
            {"filter", "status:incomplete"}
        });
        if (taskListResponse?.Tasks == null)
            throw new ApplicationException($"Error getting task lists from response: {taskListResponse?.InfoString}");
        return taskListResponse.Tasks;
    }

    public async Task<RtmTasksList> AddTaskAsync(string? authToken, string timeline, string listId, string itemToAddToList)
    {
        var addResponse = await CallRtmMethodAsync<RtmTasksAddResponse>(new Dictionary<string, string?>{
            {"method", "rtm.tasks.add"},
            {"auth_token", authToken ?? throw new ArgumentNullException(nameof(authToken))},
            {"timeline", timeline},
            {"list_id", listId},
            {"name", itemToAddToList}
        });
        if (addResponse?.List == null)
            throw new ApplicationException($"Error getting task list from add task response: {addResponse?.InfoString}");
        return addResponse.List;
    }

    public async Task<string> CreateTimelineAsync(string? authToken)
    {
        var timelineResponse = await CallRtmMethodAsync<RtmTimelinesCreateResponse>(new Dictionary<string, string?>{
            {"method", "rtm.timelines.create"},
            {"auth_token", authToken ?? throw new ArgumentNullException(nameof(authToken))}
        });
        return timelineResponse?.Timeline ?? "";
    }

    private async Task<T?> CallRtmMethodAsync<T>(IDictionary<string, string?> queryParams) where T : RtmRsp
    {
        var requestUri = new Uri(QueryHelpers.AddQueryString(config.EndpointUri.AbsoluteUri, queryParams.AddStandardParameters(config)));
        logger.LogTrace($"Calling RTM api: {requestUri}");

        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var response = await httpClient.GetAsync(requestUri);
        if (!response.IsSuccessStatusCode)
            throw new ApplicationException($"Error calling RTM: {response.StatusCode}: {(await response.Content.ReadAsStringAsync())}");
        var responseString = await response.Content.ReadAsStringAsync();
        logger.LogTrace($"Received RTM response from [{requestUri}]: {responseString}");

        var responseObj = JsonSerializer.Deserialize<RtmJsonResponse<T>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (!(responseObj?.IsSuccess ?? false))
            throw new ApplicationException($"RTM call was not successful: {responseString}");
        return responseObj.Rsp;
    }
}