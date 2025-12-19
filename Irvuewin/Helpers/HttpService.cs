using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Irvuewin.Models.Unsplash;
using Newtonsoft.Json;
using Serilog;

namespace Irvuewin.Helpers
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string url);

        private static readonly UnsplashHttpService HttpClient = new(new UnsplashHttpClientWrapper());

        static UnsplashHttpService GetUnsplashHttpService()
        {
            return HttpClient;
        }
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
        private static readonly ILogger Logger = Log.ForContext(typeof(UnsplashHttpService));
        private readonly IHttpClient _client;
        private const string BaseUrl = "https://api.unsplash.com";
        private const string Photos = "photos";
        private const string Collections = "collections";
        private const string User = "users";
        private const string Search = "search";

        public UnsplashHttpService(IHttpClient service)
        {
            _client = service;
            Logger.Debug("UnsplashApi initialized.");
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
                // TODO  System.Exception: Cannot unmarshal type TypeEnum
                return JsonConvert.DeserializeObject<T>(content, JsonHelper.Settings);
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(@"HTTP Request Error: {ExMessage}", ex.Message);
                // 返回泛型类型T的默认值(int返回0，对象返回null)
                return default(T);
            }
            catch (JsonException ex)
            {
                Logger.Error("JSON Deserialization Error: {ExMessage}", ex.Message);
                return default(T);
            }
        }

        // Get photo details by ID
        public async Task<UnsplashPhoto?> GetPhotoInfoById(string id)
        {
            var url = $"{Photos}/{id}";
            return await GetAsync<UnsplashPhoto>(url);
        }

        // Get a single page from the Editorial feed
        // Query params: page and per_page
        public async Task<List<UnsplashPhoto>?> GetEditorialFeed(UnsplashQueryParams query)
        {
            var url = $"{Photos}?{query.ToQueryString()}";
            return await GetAsync<List<UnsplashPhoto>>(url);
        }

        // Get a single collection by ID
        public async Task<UnsplashChannel?> GetChannelById(string id)
        {
            var url = $"{Collections}/{id}";
            return await GetAsync<UnsplashChannel>(url);
        }

        // Get a single page of photos in a collection
        public async Task<List<UnsplashPhoto>?> GetPhotosOfChannel(string channelId, UnsplashQueryParams? query)
        {
            var url = $"{Collections}/{channelId}/{Photos}";
            if (query != null)
            {
                url += $"?{query.ToQueryString()}";
            }
            return await GetAsync<List<UnsplashPhoto>>(url);
        }

        /// <summary>
        /// Get Random photo(s) from specific collection.
        /// </summary>
        /// <param name="channelId">Channel Id</param>
        /// <param name="count">Wallpaper count</param>
        /// <returns></returns>
        public async Task<List<UnsplashPhoto>?> GetRandomPhotoInChannel(string channelId, int count = 1)
        {
            var ori = Properties.Settings.Default.WallpaperOrientation;
            var url = $"{Photos}/random?{Collections}={channelId}";
            url = ori switch
            {
                0 => $"{url}&orientation=landscape",
                1 => $"{url}&orientation=portrait",
                // 2 => $"{url}&orientation=squarish",
                _ => url
            };
            url = $"{url}&count={count}";
            return await GetAsync<List<UnsplashPhoto>>(url);
        }

        // Get user profile by username
        public async Task<UnsplashUser?> GetUserProfile(string username)
        {
            var url = $"{User}/{username}";
            return await GetAsync<UnsplashUser>(url);
        }

        // Get first 10 collections(channels) by username (paginated by default)
        public async Task<List<UnsplashChannel>?> GetUserChannels(string username)
        {
            var url = $"{User}/{username}/{Collections}";
            return await GetAsync<List<UnsplashChannel>>(url);
        }

        // Search collections by keywords
        public async Task<UnsplashSearchResult?> SearchChannels(string keywords, UnsplashQueryParams query)
        {
            var url = $"{Search}/{Collections}?query={keywords}&{query.ToQueryString()}";
            return await GetAsync<UnsplashSearchResult>(url);
        }
    }
}