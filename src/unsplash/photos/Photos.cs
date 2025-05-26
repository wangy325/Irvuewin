
using Irvuewin.unsplash.collections;

namespace Irvuewin.unsplash.photos
{

    public abstract class Photo
    {
        public string? Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PromotedAt { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? Description { get; set; }
        public string? AltDescription { get; set; }
        public required Urls Urls { get; set; }
    }

    public class UPhoto : Photo
    {
        public string? Slug { get; set; }
        public string? Color { get; set; }
        public string? BlurHash { get; set; }
        public required Links Links { get; set; }
        public int Likes { get; set; }
        public bool LikedByUser { get; set; }
        public string? AssetType { get; set; }
        public required User User { get; set; }
        public Exif? Exif { get; set; }
        public Location? Location { get; set; }
        public Meta? Meta { get; set; }
        public bool PublicDomain { get; set; }
        public List<Tag>? Tags { get; set; }
        public int Views { get; set; }
        public int Downloads { get; set; }
        public RelatedCollections? RelatedCollections { get; set; }
    }


    public class Urls
    {
        public string? Raw { get; set; }
        public string? Full { get; set; }
        public string? Regular { get; set; }
        public string? Small { get; set; }
        public string? Thumb { get; set; }
        public string? SmallS3 { get; set; }
    }

    public class Links
    {
        public string? Self { get; set; }
        public string? Html { get; set; }
        public string? Download { get; set; }
        public string? DownloadLocation { get; set; }
    }

    public class Exif
    {
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Name { get; set; }
        public string? ExposureTime { get; set; }
        public string? Aperture { get; set; }
        public string? FocalLength { get; set; }
        public int Iso { get; set; }
    }

    public class Location
    {
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public Position? Position { get; set; }
    }

    public class Position
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class Meta
    {
        public bool Index { get; set; }
    }

    public class Tag
    {
        public required string Type { get; set; }
        public required string Title { get; set; }
    }

    public class RelatedCollections
    {
        public int Total { get; set; }
        public string? Type { get; set; }
        public List<UCollection>? Results { get; set; }
    }

    


}
