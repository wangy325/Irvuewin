using System.Windows.Documents;
using Irvuewin.Models.Unsplash;

namespace Irvuewin.Tests.Models.Unsplash;

///<summary>
///Author: wangy325
///Date: 2020/01/01 18:18:18
///Desc: 
///</summary>
public static class UnsplashDataSet
{
    public static string PhotoId = "0RbbLWm6rLk";
    public static string CollectionId = "raoebyzOILQ";
    
    public static UnsplashPhoto ExpectedPhoto = new()
    {
        Id = "0RbbLWm6rLk",
        Width = 5944,
        Height = 3949,
        Slug = "woman-in-white-tank-top-wearing-black-sunglasses-0RbbLWm6rLk"
    };

    public static UnsplashCollection ExpectedCollection = new()
    {
        Id = "raoebyzOILQ",
        Title = "Blue",
        ShareKey = "1b2525b4bfce502c199213d2e738273d",
        UpdatedAt = new DateTime(2025, 05, 22, 18, 38, 06, 00, DateTimeKind.Utc),
    };

    public static List<UnsplashPhoto> ExpectedPhotos = new()
    {
        new UnsplashPhoto
        {
            Id = "_kJjvPI8t4M",
            Width = 6000,
            Height = 4000,
            Slug = "a-bunch-of-umbrellas-that-are-outside-in-the-rain-_kJjvPI8t4M"
        },
        new UnsplashPhoto
        {
            Id = "ybNyhplDCT8",
            Width = 8192,
            Height = 5464,
            Slug = "a-blurry-image-of-blue-lights-in-the-dark-ybNyhplDCT8"
        }
    };
    
}