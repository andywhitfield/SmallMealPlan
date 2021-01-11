using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SmallMealPlan.SmallLister
{
    public class SmallListerClient : ISmallListerClient
    {
        public const string HttpClientName = nameof(SmallListerClient);
        private static JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private readonly ILogger<SmallListerClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SmallListerConfig _config;

        public SmallListerClient(ILogger<SmallListerClient> logger, IHttpClientFactory httpClientFactory, SmallListerConfig config)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<SmallListerList> GetListAsync(string refreshToken, string listId)
        {
            string accessToken;
            using var httpClient = _httpClientFactory.CreateClient(HttpClientName);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(_config.BaseUri, "/api/v1/token"));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{_config.AppKey}:{_config.AppSecret}")));
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(new { refreshToken }), Encoding.UTF8, "application/json");
            using (var response = await httpClient.SendAsync(requestMessage))
            {
                if (!response.IsSuccessStatusCode)
                    throw new ApplicationException($"Error calling SmallLister: {response.StatusCode}: {(await response.Content.ReadAsStringAsync())}");

                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogTrace($"Received SmallLister token response: {responseString}");

                var tokenResponse = JsonSerializer.Deserialize<SmallListerTokenResponse>(responseString, DefaultJsonSerializerOptions);
                if (tokenResponse.AccessToken == null)
                    throw new ApplicationException($"Could not get access token from SmallLister: {response.StatusCode}: {responseString}");

                accessToken = tokenResponse.AccessToken;
            }

            requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(_config.BaseUri, $"/api/v1/list/{listId}"));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using (var response = await httpClient.SendAsync(requestMessage))
            {
                if (!response.IsSuccessStatusCode)
                    throw new ApplicationException($"Error calling SmallLister: {response.StatusCode}: {(await response.Content.ReadAsStringAsync())}");

                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogTrace($"Received SmallLister list response: {responseString}");

                return JsonSerializer.Deserialize<SmallListerList>(responseString, DefaultJsonSerializerOptions);
            }
        }

        public async Task AddItemAsync(string refreshToken, string listId, string itemToAddToList)
        {
            string accessToken;
            using var httpClient = _httpClientFactory.CreateClient(HttpClientName);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(_config.BaseUri, "/api/v1/token"));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{_config.AppKey}:{_config.AppSecret}")));
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(new { refreshToken }), Encoding.UTF8, "application/json");
            using (var response = await httpClient.SendAsync(requestMessage))
            {
                if (!response.IsSuccessStatusCode)
                    throw new ApplicationException($"Error calling SmallLister: {response.StatusCode}: {(await response.Content.ReadAsStringAsync())}");

                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogTrace($"Received SmallLister token response: {responseString}");

                var tokenResponse = JsonSerializer.Deserialize<SmallListerTokenResponse>(responseString, DefaultJsonSerializerOptions);
                if (tokenResponse.AccessToken == null)
                    throw new ApplicationException($"Could not get access token from SmallLister: {response.StatusCode}: {responseString}");

                accessToken = tokenResponse.AccessToken;
            }

            requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(_config.BaseUri, "/api/v1/item"));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(new SmallListerItem
            {
                ListId = listId,
                Description = itemToAddToList
            }), Encoding.UTF8, "application/json");
            using (var response = await httpClient.SendAsync(requestMessage))
            {
                if (!response.IsSuccessStatusCode)
                    throw new ApplicationException($"Error calling SmallLister: {response.StatusCode}: {(await response.Content.ReadAsStringAsync())}");
            }
        }

        public async Task<IEnumerable<SmallListerList>> GetListsAsync(string refreshToken)
        {
            string accessToken;
            using var httpClient = _httpClientFactory.CreateClient(HttpClientName);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(_config.BaseUri, "/api/v1/token"));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{_config.AppKey}:{_config.AppSecret}")));
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(new { refreshToken }), Encoding.UTF8, "application/json");
            using (var response = await httpClient.SendAsync(requestMessage))
            {
                if (!response.IsSuccessStatusCode)
                    throw new ApplicationException($"Error calling SmallLister: {response.StatusCode}: {(await response.Content.ReadAsStringAsync())}");

                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogTrace($"Received SmallLister token response: {responseString}");

                var tokenResponse = JsonSerializer.Deserialize<SmallListerTokenResponse>(responseString, DefaultJsonSerializerOptions);
                if (tokenResponse.AccessToken == null)
                    throw new ApplicationException($"Could not get access token from SmallLister: {response.StatusCode}: {responseString}");

                accessToken = tokenResponse.AccessToken;
            }

            requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(_config.BaseUri, "/api/v1/list"));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using (var response = await httpClient.SendAsync(requestMessage))
            {
                if (!response.IsSuccessStatusCode)
                    throw new ApplicationException($"Error calling SmallLister: {response.StatusCode}: {(await response.Content.ReadAsStringAsync())}");

                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogTrace($"Received SmallLister lists response: {responseString}");

                var listsResponse = JsonSerializer.Deserialize<SmallListerListsResponse>(responseString, DefaultJsonSerializerOptions);
                return listsResponse.Lists;
            }
        }
    }
}