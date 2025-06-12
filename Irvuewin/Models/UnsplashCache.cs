using System.IO;
using Irvuewin.Helpers;
using Irvuewin.Helpers.Utils;
using Irvuewin.Models.Unsplash;
using Newtonsoft.Json;

namespace Irvuewin.Models
{
    public static class UnsplashCache
    {
        // TODO Necessary?
        public static List<UnsplashChannel> CachedChannels = [];
        public static readonly Dictionary<PhotosCachePageIndex, List<UnsplashPhoto>> CachedPhotos = new();


        public static async Task CacheChannelsAsync(List<UnsplashChannel> channels)
        {
            CachedChannels = channels;
            try
            {
                await File.WriteAllTextAsync(FileUtils.CachePath("channels"),
                    JsonConvert.SerializeObject(channels, JsonHelper.Settings));
                Console.WriteLine($@"Saved {channels.Count} channels to cache");
            }
            catch (Exception e)
            {
                Console.WriteLine($@"{0}: {e.Message}");
            }
        }

        // Load channels from cache
        // May return empty list or null
        public static async Task<List<UnsplashChannel>?> LoadChannelsAsync()
        {
            try
            {
                var filePath = FileUtils.CachePath("channels");
                if (!File.Exists(filePath)) return null;
                var channels = await File.ReadAllTextAsync(filePath);
                CachedChannels = JsonConvert.DeserializeObject<List<UnsplashChannel>>(
                    channels,
                    JsonHelper.Settings) ?? [];
                Console.WriteLine($@"Loaded {CachedChannels.Count} channels from cache");
                return CachedChannels;
            }
            catch (Exception e)
            {
                Console.WriteLine($@"{0}: {e.Message}");
                return null;
            }
        }

        public static async Task CachePhotosAsync(PhotosCachePageIndex index, List<UnsplashPhoto> photos)
        {
            CachedPhotos[index] = photos;
            try
            {
                var filePath = FileUtils.CachePath($"photos_{index.ChannelId}_{index.PageIndex}");
                await File.WriteAllTextAsync(filePath,
                    JsonConvert.SerializeObject(photos, JsonHelper.Settings));
                Console.WriteLine($@"Saved {photos.Count} photos to cache");
            }
            catch (Exception e)
            {
                Console.WriteLine($@"{0}: {e.Message}");
            }
        }

        // Get channel's photo list from cache file
        // Returns null if cache file does not exist or exception occurs
        public static async Task<List<UnsplashPhoto>?> LoadPhotosAsync(PhotosCachePageIndex index)
        {
            var filePath = FileUtils.CachePath($"photos_{index.ChannelId}_{index.PageIndex}");
            if (!File.Exists(filePath)) return null;
            try
            {
                var photos = await File.ReadAllTextAsync(filePath);
                CachedPhotos[index] = JsonConvert.DeserializeObject<List<UnsplashPhoto>>(
                    photos,
                    JsonHelper.Settings) ?? [];
                Console.WriteLine(
                    $@"Loaded {CachedPhotos[index].Count} photos from cache for channel {index.ChannelId}:{index.PageIndex}");
                return CachedPhotos[index];
            }
            catch (Exception e)
            {
                Console.WriteLine($@"{0}: {e.Message}");
                return null;
            }
        }

        public static async Task<bool> CacheChannelSequence(Dictionary<string, int> cachedWallpaperSequence)
        {
            var filePath = FileUtils.CachePath("sequence");
            try
            {
                await File.WriteAllTextAsync(filePath,
                    JsonConvert.SerializeObject(cachedWallpaperSequence, JsonHelper.Settings));
                Console.WriteLine($@"Saved {cachedWallpaperSequence.Count} sequence to cache");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task<Dictionary<string, int>?> LoadChannelSequence()
        {
            var filePath = FileUtils.CachePath("sequence");
            try
            {
                if (!File.Exists(filePath)) return null;
                var sequence = await File.ReadAllTextAsync(filePath);
                var cachedSequence = JsonConvert.DeserializeObject<Dictionary<string, int>>(
                    sequence,
                    JsonHelper.Settings) ?? new Dictionary<string, int>();
                Console.WriteLine($@"Loaded {cachedSequence.Count} sequence from cache");
                return cachedSequence;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public class PhotosCachePageIndex
    {
        public required string ChannelId { get; set; }
        public required int PageIndex { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not PhotosCachePageIndex other) return false;
            return PageIndex == other.PageIndex && ChannelId == other.ChannelId;
        }

        public override int GetHashCode()
        {
            return PageIndex.GetHashCode() ^ ChannelId.GetHashCode();
        }
    }
}