// YApi QuickType插件生成，具体参考文档:https://plugins.jetbrains.com/plugin/18847-yapi-quicktype/documentation
#pragma warning disable CS8618

using Irvuewin.Helpers;
using Newtonsoft.Json;

namespace Irvuewin.Models.Unsplash
{
    public partial class UnsplashPhoto
    {
        

        [JsonProperty("slug")] public string Slug { get; set; }

        [JsonProperty("color")] public string Color { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("alt_description")] public string AltDescription { get; set; }

        [JsonProperty("width")] public long Width { get; set; }

        [JsonProperty("height")] public long Height { get; set; }

        

        [JsonProperty("links")] public Links Links { get; set; }

        [JsonProperty("user")] public UnsplashUser User { get; set; }

        [JsonProperty("downloads")] public long Downloads { get; set; }

        [JsonProperty("views")] public long Views { get; set; }

        [JsonProperty("likes")] public long Likes { get; set; }

        [JsonProperty("liked_by_user")] public bool LikedByUser { get; set; }
        
        [JsonProperty("tags")] public List<Tag> Tags { get; set; }

        [JsonProperty("created_at")] public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("promoted_at")] public DateTimeOffset PromotedAt { get; set; }

        [JsonProperty("asset_type")] public AssetType AssetType { get; set; }

        [JsonProperty("location")] public Location Location { get; set; }

        [JsonProperty("exif")] public Exif Exif { get; set; }
    }

    public partial class Urls
    {
        [JsonProperty("small")] public Uri Small { get; set; }

        [JsonProperty("small_s3")] public Uri SmallS3 { get; set; }

        [JsonProperty("thumb")] public Uri Thumb { get; set; }

        [JsonProperty("raw")] public Uri Raw { get; set; }

        [JsonProperty("regular")] public Uri Regular { get; set; }

        [JsonProperty("full")] public Uri Full { get; set; }
    }

    public partial class Exif
    {
        [JsonProperty("exposure_time")] public string ExposureTime { get; set; }

        [JsonProperty("aperture")] public string Aperture { get; set; }

        [JsonProperty("focal_length")] public string FocalLength { get; set; }

        [JsonProperty("iso")] public long Iso { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("model")] public string Model { get; set; }

        [JsonProperty("make")] public string Make { get; set; }
    }

    public partial class Links
    {
        [JsonProperty("download")] public Uri Download { get; set; }

        [JsonProperty("download_location")] public Uri DownloadLocation { get; set; }

        [JsonProperty("self")] public Uri Self { get; set; }

        [JsonProperty("html")] public Uri Html { get; set; }
    }

    public partial class Location
    {
        [JsonProperty("country")] public string Country { get; set; }

        [JsonProperty("city")] public string City { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("position")] public Position Position { get; set; }
    }

    public partial class Position
    {
        [JsonProperty("latitude")] public double Latitude { get; set; }

        [JsonProperty("longitude")] public double Longitude { get; set; }
    }

    public partial class Tag
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("title")] public string Title { get; set; }
    }

    public enum AssetType
    {
        Photo
    };
    


    public partial class UnsplashPhoto
    {
        public static UnsplashPhoto? FromJson(string json) =>
            JsonConvert.DeserializeObject<UnsplashPhoto>(json, JsonHelper.Settings);
    }

    public static class SerializeUnsplashPhoto
    {
        public static string ToJson(this UnsplashPhoto self) =>
            JsonConvert.SerializeObject(self, JsonHelper.Settings);
    }

    public partial class UnsplashPhoto
    {
        [JsonProperty("isFiltered")]public bool IsFiltered { get; set; }
    }

    public partial class UnsplashPhoto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("urls")] public Urls Urls { get; set; }
    }
}