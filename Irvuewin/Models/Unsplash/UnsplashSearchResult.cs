using Newtonsoft.Json;

namespace Irvuewin.Models.Unsplash;

public partial class UnsplashSearchResult
{
    [JsonProperty("total")]public int Total;
    [JsonProperty("total_pages")]public int TotalPages;
    [JsonProperty("results")]public List<UnsplashChannel> Results;
}