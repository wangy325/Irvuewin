using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Irvuewin.Models.Unsplash;
using Newtonsoft.Json;

namespace Irvuewin.Helpers
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string url);

        HttpClient Client();
    }

    public class HttpClientWrapper : IHttpClient
    {
        private readonly HttpClient _httpClient = new();

        public Task<HttpResponseMessage> GetAsync(string url) => _httpClient.GetAsync(url);

        public HttpClient Client()
        {
            return _httpClient;
        }
    }

    public class UnsplashHttpService
    {
        private readonly string _apiKey = "Client-ID gOjCXnSXiHRasWqeABszxQQCsBJgceXSjHdZYVTfZR8";
        private readonly IHttpClient _client;
        private const string BaseUrl = "https://api.unsplash.com";

        public UnsplashHttpService(IHttpClient service)
        {
            _client = service;
            if (service.Client() is { } client)
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Authorization", _apiKey);
                client.DefaultRequestHeaders.Add("Accept-Version", "v1");
            }

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

        public async Task<UnsplashPhoto?> GetPhotoInfoById(string id)
        {
            var url = $"photos/{id}";
            return await GetAsync<UnsplashPhoto>(url);
        }
    }
}