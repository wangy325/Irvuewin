#pragma warning disable CS8618
using Irvuewin.Helpers;
using Newtonsoft.Json;

namespace Irvuewin.Models.Unsplash
{
    public partial class UnsplashUser
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("username")] public string Username { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("first_name")] public string FirstName { get; set; }

        [JsonProperty("last_name")] public string LastName { get; set; }

        [JsonProperty("portfolio_url")] public Uri PortfolioUrl { get; set; }

        [JsonProperty("bio")] public string Bio { get; set; }

        [JsonProperty("location")] public string Location { get; set; }

        [JsonProperty("links")] public UserLinks Links { get; set; }

        [JsonProperty("profile_image")] public ProfileImage ProfileImage { get; set; }

        [JsonProperty("total_collections")] public long TotalCollections { get; set; }

        [JsonProperty("total_likes")] public long TotalLikes { get; set; }

        [JsonProperty("total_photos")] public long TotalPhotos { get; set; }

        [JsonProperty("total_promoted_photos")]
        public long TotalPromotedPhotos { get; set; }

        [JsonProperty("total_promoted_illustrations")]
        public long TotalPromotedIllustrations { get; set; }

        [JsonProperty("total_illustrations")] public long TotalIllustrations { get; set; }

        [JsonProperty("instagram_username")] public string InstagramUsername { get; set; }
    }

    public class UserLinks
    {
        [JsonProperty("portfolio")] public Uri Portfolio { get; set; }

        [JsonProperty("self")] public Uri Self { get; set; }

        [JsonProperty("html")] public Uri Html { get; set; }

        [JsonProperty("photos")] public Uri Photos { get; set; }

        [JsonProperty("likes")] public Uri Likes { get; set; }
    }

    public class ProfileImage
    {
        [JsonProperty("small")] public Uri Small { get; set; }

        [JsonProperty("large")] public Uri Large { get; set; }

        [JsonProperty("medium")] public Uri Medium { get; set; }
    }

    public partial class UnsplashUser
    {
        public static UnsplashUser? FromJson(string json) =>
            JsonConvert.DeserializeObject<UnsplashUser>(json, JsonHelper.Settings);

        // [JsonIgnore] public bool Blocked { get; set; }
    }

    public static class SerializeUnsplashUser
    {
        public static string ToJson(this UnsplashUser self) =>
            JsonConvert.SerializeObject(self, JsonHelper.Settings);
    }
}