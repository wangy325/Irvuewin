
using Irvuewin.unsplash.photos;

namespace Irvuewin.unsplash.collections
{
    public class UCollection
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public object Description { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime LastCollectedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Featured { get; set; }
        public int TotalPhotos { get; set; }
        public bool Private { get; set; }
        public string? ShareKey { get; set; }
        public Links Links { get; set; }
        public User User { get; set; }
        public UPhoto CoverPhoto { get; set; }
        public List<PreviewPhoto> PreviewPhotos { get; set; }
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
        public  string Id { get; set; }
        public string? Slug { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? BlurHash { get; set; }
        public string? AssetType { get; set; }
        public Urls Urls { get; set; }
    }

}
