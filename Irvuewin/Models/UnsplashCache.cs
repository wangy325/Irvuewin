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
        // public static List<UnsplashChannel> CachedChannels = [];
        // This may take huge memory usage
        // public static readonly Dictionary<PhotosCachePageIndex, List<UnsplashPhoto>> CachedPhotos = new();


        public static async Task CacheChannelsAsync(List<UnsplashChannel> channels)
        {
            // CachedChannels = channels;
            try
            {
                await File.WriteAllTextAsync(FileUtils.CachePath(FileUtils.AppDataFolder, "channels"),
                    JsonConvert.SerializeObject(channels, JsonHelper.Settings));
                // Console.WriteLine($@"Saved {channels.Count} channels to cache");
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
                var filePath = FileUtils.CachePath(FileUtils.AppDataFolder, "channels");
                if (!File.Exists(filePath)) return null;
                var channelString = await File.ReadAllTextAsync(filePath);
                /*CachedChannels = JsonConvert.DeserializeObject<List<UnsplashChannel>>(
                    channels,
                    JsonHelper.Settings) ?? [];
                Console.WriteLine($@"Loaded {CachedChannels.Count} channels from cache");
                return CachedChannels;*/
                var channels = JsonConvert.DeserializeObject<List<UnsplashChannel>>(
                    channelString,
                    JsonHelper.Settings) ?? [];
                return channels;
            }
            catch (Exception e)
            {
                Console.WriteLine($@"{0}: {e.Message}");
                return null;
            }
        }

        public static async Task CachePhotosAsync(PhotosCachePageIndex index, List<UnsplashPhoto> photos)
        {
            // CachedPhotos[index] = photos;
            try
            {
                // Full path name: appDataFolder/photos/channelId/filename
                var dir = FileUtils.CreateDir(FileUtils.CachedPhotoBaseFolder, index.ChannelId);
                var filePath = FileUtils.CachePath(dir, $"{IAppConst.CachedPhotosNamePrefix}{index.PageIndex}");
                await File.WriteAllTextAsync(filePath,
                    JsonConvert.SerializeObject(photos, JsonHelper.Settings));
                // Console.WriteLine($@"Saved {photos.Count} photos to cache");
            }
            catch (Exception e)
            {
                Console.WriteLine($@"{0}: {e.Message}");
            }
        }

        // Get sharding of channel's photo list from cache file
        // Returns null if cache file does not exist or exception occurs
        public static async Task<List<UnsplashPhoto>?> LoadPhotosShardAsync(PhotosCachePageIndex index)
        {
            var filePath = FileUtils.CachePath(
                Path.Combine(FileUtils.CachedPhotoBaseFolder, index.ChannelId),
                $"{IAppConst.CachedPhotosNamePrefix}{index.PageIndex}");
            if (!File.Exists(filePath)) return null;
            try
            {
                var photoString = await File.ReadAllTextAsync(filePath);
                /*CachedPhotos[index] = JsonConvert.DeserializeObject<List<UnsplashPhoto>>(
                    photoString,
                    JsonHelper.Settings) ?? [];*/
                // Console.WriteLine(
                //     $@"Loaded {CachedPhotos[index].Count} photos from cache for channel {index.ChannelId}:{index.PageIndex}");
                // return CachedPhotos[index];
                var photos = JsonConvert.DeserializeObject<List<UnsplashPhoto>>(
                    photoString,
                    JsonHelper.Settings) ?? [];
                return photos;
            }
            catch (Exception e)
            {
                Console.WriteLine($@"{0}: {e.Message}");
                return null;
            }
        }

        // Load all cached photos count for specified channel
        public static async Task<int> LoadPhotoCountAsync(string channelId)
        {
            var folder = Path.Combine(FileUtils.CachedPhotoBaseFolder, channelId);
            if (!Directory.Exists(folder)) return 0;
            var files = Directory.GetFiles(folder);
            if (files.Length == 0) return 0;
            var maxShard = files
                .Select(file => Convert.ToInt32(Path.GetFileNameWithoutExtension(file).Split('_')[1].Split('.')[0]))
                .Prepend(0)
                .Max();

            var latestShard = Path.Combine(folder,
                $"{IAppConst.CachedPhotosNamePrefix}{maxShard}.{IAppConst.CachedPhotosNameSuffix}");
            var json = await File.ReadAllTextAsync(latestShard);
            var latestShardCount =
                JsonConvert.DeserializeObject<List<UnsplashPhoto>>(json, JsonHelper.Settings)
                    ?.Count;

            return (int)((files.Length - 1) * IAppConst.PageSize + latestShardCount)!;
        }

        public static async Task<bool> CacheChannelSequence(Dictionary<string, int> cachedWallpaperSequence)
        {
            var filePath = FileUtils.CachePath(FileUtils.AppDataFolder, "sequence");
            try
            {
                await File.WriteAllTextAsync(filePath,
                    JsonConvert.SerializeObject(cachedWallpaperSequence, JsonHelper.Settings));
                // Console.WriteLine($@"Saved {cachedWallpaperSequence.Count} sequence to cache");
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
            var filePath = FileUtils.CachePath(FileUtils.AppDataFolder, "sequence");
            try
            {
                if (!File.Exists(filePath)) return null;
                var sequence = await File.ReadAllTextAsync(filePath);
                var cachedSequence = JsonConvert.DeserializeObject<Dictionary<string, int>>(
                    sequence,
                    JsonHelper.Settings) ?? new Dictionary<string, int>();
                // Console.WriteLine($@"Loaded {cachedSequence.Count} sequence from cache");
                return cachedSequence;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void UnCacheChannelPhotos(string channelId)
        {
            // Remove cached photos sharding
            /*var keysToRemove = CachedPhotos
                .Where(pair => pair.Key.ChannelId == channelId)
                .Select(pair => pair.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                CachedPhotos.Remove(key);
            }*/

            // Remove disk caches
            var dir = Path.Combine(FileUtils.CachedPhotoBaseFolder, channelId);
            FileUtils.DeleteFolder(dir);
        }
    }

    public class PhotosCachePageIndex
    {
        public required string ChannelId { get; init; }
        public required int PageIndex { get; init; }

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