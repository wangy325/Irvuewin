
using Irvuewin.unsplash.photos;

namespace Irvuewin.unsplash.collections
{
    public class Collection
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public required object Description { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime LastCollectedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Featured { get; set; }
        public int TotalPhotos { get; set; }
        public bool Private { get; set; }
        public string? ShareKey { get; set; }
        public required Links Links { get; set; }
        public required User User { get; set; }
        public required UnsplashPhoto CoverPhoto { get; set; }
        public required List<PreviewPhoto> PreviewPhotos { get; set; }
    }

    public class Links
    {
        public string? Self { get; set; }
        public string? Html { get; set; }
        public string? Photos { get; set; }
        public string? Related { get; set; }
    }


    public class PreviewPhoto
    {
        public required string Id { get; set; }
        public string? Slug { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? BlurHash { get; set; }
        public string? AssetType { get; set; }
        public required Urls Urls { get; set; }
    }

}
