using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Irvuewin.Helpers;
using Newtonsoft.Json;

namespace Irvuewin.Models.Unsplash

{
    ///<summary>
    ///Author: wangy325
    ///Date: 2020/01/01 18:12:10
    ///Desc: 
    ///</summary>
    public class UnsplashApi
    {
        private readonly string _apiKey = "Client-ID gOjCXnSXiHRasWqeABszxQQCsBJgceXSjHdZYVTfZR8";
        private readonly IHttpService _service;
        private const string BaseUrl = "https://api.unsplash.com";

        public UnsplashApi(IHttpService service)
        {
            // if (service is not HttpClientWrapper wrapper) return;
            _service = service;
            var client = service.Client();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Authorization", _apiKey);
            client.DefaultRequestHeaders.Add("Accept-Version", "v1");
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
                var response = await _service.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($@"HTTP Request Error: {ex.Message}");
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