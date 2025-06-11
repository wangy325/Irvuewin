// YApi QuickType插件生成，具体参考文档:https://plugins.jetbrains.com/plugin/18847-yapi-quicktype/documentation

using Irvuewin.Helpers;
using Newtonsoft.Json;

namespace Irvuewin.Models.Unsplash
{
    public partial class UnsplashChannel
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("published_at")] public DateTimeOffset PublishedAt { get; set; }

        [JsonProperty("last_collected_at")] public DateTimeOffset LastCollectedAt { get; set; }

        [JsonProperty("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("featured")] public bool Featured { get; set; }

        [JsonProperty("private")] public bool Private { get; set; }

        [JsonProperty("total_photos")] public int TotalPhotos { get; set; }

        [JsonProperty("share_key")] public string ShareKey { get; set; }

        [JsonProperty("links")] public Links Links { get; set; }

        // Nullable
        [JsonProperty("cover_photo")] public UnsplashPhoto CoverPhoto { get; set; }
        
        // Nullable
        [JsonProperty("preview_photos")] public List<PreviewPhoto> PreviewPhotos { get; set; }


        [JsonProperty("user")] public UnsplashUser User { get; set; }
    }


    public partial class Links
    {
        [JsonProperty("related")] public Uri Related { get; set; }

        [JsonProperty("photos")] public Uri Photos { get; set; }
    }


    public partial class PreviewPhoto
    {
        [JsonProperty("urls")] public Urls Urls { get; set; }

        [JsonProperty("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("blur_hash")] public string BlurHash { get; set; }

        [JsonProperty("asset_type")] public string AssetType { get; set; }

        [JsonProperty("created_at")] public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("slug")] public string Slug { get; set; }
    }


    public partial class UnsplashChannel
    {
        public static UnsplashChannel? FromJson(string json) =>
            JsonConvert.DeserializeObject<UnsplashChannel>(json, JsonHelper.Settings);
    }

    public static class SerializeUnsplashCollection
    {
        public static string ToJson(this UnsplashChannel self) =>
            JsonConvert.SerializeObject(self, (JsonSerializerSettings?)JsonHelper.Settings);
    }
    
}