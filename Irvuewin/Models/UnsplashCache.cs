using System.Diagnostics;
using System.IO;
using Irvuewin.Helpers;
using Irvuewin.Helpers.Utils;
using Irvuewin.Models.Unsplash;
using Newtonsoft.Json;

namespace Irvuewin.Models
{
    public static class UnsplashCache
    {
        private static List<UnsplashChannel> _savedChannels = [];
        private static Dictionary<PhotosCachePageIndex, List<UnsplashPhoto>> _savedPhotos = new();


        public static async Task<bool> SaveChannelsASync(List<UnsplashChannel> channels)
        {
            _savedChannels = channels;
            try
            {
                await File.WriteAllTextAsync(FileUtils.CachePath("channels"),
                    JsonConvert.SerializeObject(channels, JsonHelper.Settings));
                Debug.WriteLine($"Saved {channels.Count} channels to cache");
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($@"{0}: {e.Message}");
                return false;
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
                _savedChannels = JsonConvert.DeserializeObject<List<UnsplashChannel>>(
                    channels,
                    JsonHelper.Settings) ?? [];
                Debug.WriteLine($"Loaded {_savedChannels.Count} channels from cache");
                return _savedChannels;
            }
            catch (Exception e)
            {
                Debug.WriteLine($@"{0}: {e.Message}");
                return null;
            }
        }
        
        public static async Task<bool> SavePhotosAsync(PhotosCachePageIndex index, List<UnsplashPhoto> photos)
        {
            try
            {
                _savedPhotos[index] = photos;
                var filePath = FileUtils.CachePath($"photos_{index.ChannelId}_{index.PageIndex}");
                await File.WriteAllTextAsync(filePath,
                    JsonConvert.SerializeObject(photos, JsonHelper.Settings));
                Debug.WriteLine($"Saved {photos.Count} photos to cache");
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($@"{0}: {e.Message}");
                return false;
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
                _savedPhotos[index] = JsonConvert.DeserializeObject<List<UnsplashPhoto>>(
                    photos,
                    JsonHelper.Settings) ?? [];
                Debug.WriteLine($"Loaded {_savedPhotos[index].Count} photos from cache for channel {index.ChannelId}:{index.PageIndex}");
                return _savedPhotos[index];
            }
            catch (Exception e)
            {
                Debug.WriteLine($@"{0}: {e.Message}");
                return null;
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