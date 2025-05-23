using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irvuewin.src.unsplash
{

    public abstract class User 
    { 
        public required string Id { get; set; }
        public required string Username { get; set; }
        public required string Name { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public ProfileImage? ProfileImage { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
    public class UnsplashUser : User
    {
        public object? TwitterUsername { get; set; }
        public string? PortfolioUrl { get; set; }
        public required UserLinks Links { get; set; }
        public string? InstagramUsername { get; set; }
        public int TotalCollections { get; set; }
        public int TotalLikes { get; set; }
        public int TotalPhotos { get; set; }
        public int TotalPromotedPhotos { get; set; }
        public int TotalIllustrations { get; set; }
        public int TotalPromotedIllustrations { get; set; }
        public bool AcceptedTos { get; set; }
        public bool ForHire { get; set; }
        public Social? Social { get; set; }
    }

    public class UserLinks
    {
        public string? Self { get; set; }
        public string? Html { get; set; }
        public string? Photos { get; set; }
        public string? Likes { get; set; }
        public string? Portfolio { get; set; }
    }

    public class ProfileImage
    {
        public string? Small { get; set; }
        public string? Medium { get; set; }
        public string?  Large { get; set; }
    }

    public class Social
    {
        public string? InstagramUsername { get; set; }
        public string? PortfolioUrl { get; set; }
        public object? TwitterUsername { get; set; }
        public object? PaypalEmail { get; set; }
    }
}
