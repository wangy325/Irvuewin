using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Irvuewin.unsplash.photos;
using Newtonsoft.Json;

namespace Irvuewin.unsplash

{
    ///<summary>
    ///Author: wangy325
    ///Date: 2020/01/01 18:12:10
    ///Desc: 
    ///</summary>

    public class UnsplashApi
    {
        private readonly string _apiKey = "Client-ID gOjCXnSXiHRasWqeABszxQQCsBJgceXSjHdZYVTfZR8";
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.unsplash.com/";

        public UnsplashApi( HttpClient? httpClient = null)
        {
            _httpClient = httpClient?? new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept-Version", "v1");
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
                var response = await _httpClient.GetAsync(url);
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
        
        public async Task<UPhoto?> GetPhotoInfoById(string id)
        {
            return await GetAsync<UPhoto>(id);
        }
    }
    
    
}