using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Documents;
using Irvuewin.Models.Unsplash;
using Newtonsoft.Json;

namespace Irvuewin.Helpers
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string url);
    }

    public class UnsplashHttpClientWrapper : IHttpClient
    {
        private readonly HttpClient _httpClient = new();
        private readonly string _apiKey = Properties.Settings.Default.DefaultUnsplashApiKey;

        public UnsplashHttpClientWrapper()
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Authorization", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept-Version", "v1");
        }

        public Task<HttpResponseMessage> GetAsync(string url) => _httpClient.GetAsync(url);
    }

    public class UnsplashHttpService
    {
        private readonly IHttpClient _client;
        private const string BaseUrl = "https://api.unsplash.com";

        public UnsplashHttpService(IHttpClient service)
        {
            _client = service;
            Debug.WriteLine("UnsplashApi initialized.");
        }

        public async Task<T?> GetAsync<T>(string endpoint, string? query = null)
        {
            var url = $"{BaseUrl}/{endpoint}";
            if (!string.IsNullOrEmpty(query))
            {
                url += $"?{query}";
            }

            try
            {
                var response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content, JsonHelper.Settings);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($@"HTTP Request Error: {ex.Message}");
                // 返回泛型类型T的默认值(int返回0，对象返回null)
                return default(T);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"JSON Deserialization Error: {ex.Message}");
                return default(T);
            }
        }

        // Get photo details by ID
        public async Task<UnsplashPhoto?> GetPhotoInfoById(string id)
        {
            var url = $"photos/{id}";
            return await GetAsync<UnsplashPhoto>(url);
        }
        
        // Get a single page from the Editorial feed
        // Query params: page and per_page
        public async Task<List<UnsplashPhoto>?> GetEditorialFeed(UnsplashQueryParams query)
        {
            var url = $"photos?{query.ToQueryString()}";
            return await GetAsync<List<UnsplashPhoto>>(url);
        }
        
        // Get a single collection by ID
        public async Task<UnsplashChannel?> GetChannelById(string id)
        {
            var url = $"collections/{id}";
            return await GetAsync<UnsplashChannel>(url);
        }

        // Get a single page of photos in a collection
        public async Task<List<UnsplashPhoto>?> GetPhotosOfChannel(string channelId, UnsplashQueryParams query)
        {
            var url = $"collections/{channelId}/photos?{query.ToQueryString()}";
            return await GetAsync<List<UnsplashPhoto>>(url);
        }

    }
}