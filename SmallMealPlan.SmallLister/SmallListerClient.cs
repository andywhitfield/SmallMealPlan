using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SmallMealPlan.SmallLister;

public class SmallListerClient(ILogger<SmallListerClient> logger, IHttpClientFactory httpClientFactory, SmallListerConfig config)
    : ISmallListerClient
{
    public const string HttpClientName = nameof(SmallListerClient);
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    private string? _accessToken;

    public async Task<SmallListerList> GetListAsync(string? refreshToken, string listId)
    {
        var responseString = await CallApiAsync(refreshToken, HttpMethod.Get, $"/api/v1/list/{listId}");
        return JsonSerializer.Deserialize<SmallListerList>(responseString, DefaultJsonSerializerOptions)
            ?? throw new InvalidOperationException($"Could not get small:lister list from response: {responseString}");
    }

    public Task AddItemAsync(string? refreshToken, string listId, string itemToAddToList) =>
        CallApiAsync(refreshToken, HttpMethod.Post, "/api/v1/item", new SmallListerItem
        {
            ListId = listId,
            Description = itemToAddToList
        });
    
    public Task DeleteItemAsync(string? refreshToken, string listId, string itemToAddToList) =>
        CallApiAsync(refreshToken, HttpMethod.Delete, "/api/v1/item", new SmallListerItem
        {
            ListId = listId,
            Description = itemToAddToList
        });

    public async Task<IEnumerable<SmallListerList>> GetListsAsync(string? refreshToken)
    {
        var responseString = await CallApiAsync(refreshToken, HttpMethod.Get, "/api/v1/list");
        var listsResponse = JsonSerializer.Deserialize<SmallListerListsResponse>(responseString, DefaultJsonSerializerOptions);
        return listsResponse?.Lists ?? Enumerable.Empty<SmallListerList>();
    }

    public async Task RegisterWebhookAsync(string? refreshToken, Uri webhookBaseUri, string userId)
    {
        await UnregisterWebhookAsync(refreshToken);
        await CallApiAsync(refreshToken, HttpMethod.Post, "/api/v1/webhook", new { Webhook = new Uri(webhookBaseUri, $"api/webhook/{userId}/smalllister/list"), WebhookType = "ListChange" });
        await CallApiAsync(refreshToken, HttpMethod.Post, "/api/v1/webhook", new { Webhook = new Uri(webhookBaseUri, $"api/webhook/{userId}/smalllister/listitem"), WebhookType = "ListItemChange" });
    }

    public async Task UnregisterWebhookAsync(string? refreshToken)
    {
        await CallApiAsync(refreshToken, HttpMethod.Delete, "/api/v1/webhook/ListChange");
        await CallApiAsync(refreshToken, HttpMethod.Delete, "/api/v1/webhook/ListItemChange");
    }

    private async Task<string> GetOrCreateAccessTokenAsync(HttpClient httpClient, string? refreshToken)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);
        if (string.IsNullOrEmpty(_accessToken))
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(config.BaseUri, "/api/v1/token"));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{config.AppKey}:{config.AppSecret}")));
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(new { refreshToken }), Encoding.UTF8, "application/json");
            using var response = await httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error calling SmallLister: {response.StatusCode}: {(await response.Content.ReadAsStringAsync())}");

            var responseString = await response.Content.ReadAsStringAsync();
            logger.LogTrace($"Received SmallLister token response: {responseString}");

            var tokenResponse = JsonSerializer.Deserialize<SmallListerTokenResponse>(responseString, DefaultJsonSerializerOptions);
            if (tokenResponse?.AccessToken == null)
                throw new ApplicationException($"Could not get access token from SmallLister: {response.StatusCode}: {responseString}");

            _accessToken = tokenResponse.AccessToken;
        }

        return _accessToken;
    }

    private async Task<string> CallApiAsync(string? refreshToken, HttpMethod method, string uri, object? jsonContent = null)
    {
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var accessToken = await GetOrCreateAccessTokenAsync(httpClient, refreshToken);

        var requestMessage = new HttpRequestMessage(method, new Uri(config.BaseUri, uri));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (jsonContent != null)
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(jsonContent), Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode)
            throw new ApplicationException($"Error calling SmallLister: {response.StatusCode}: {(await response.Content.ReadAsStringAsync())}");

        var responseString = await response.Content.ReadAsStringAsync();
        logger.LogTrace($"Received SmallLister list response: {responseString}");
        return responseString;
    }
}