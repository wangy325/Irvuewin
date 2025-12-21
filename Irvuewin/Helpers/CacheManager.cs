using FastCache;
using FastCache.Collections;
using System.IO;
using Irvuewin.Models.Unsplash;
using Irvuewin.Helpers.Utils;
using Newtonsoft.Json;
using Serilog;
using static Irvuewin.Helpers.IAppConst;


namespace Irvuewin.Helpers
{
    /// <summary>
    /// In-memory cache helper
    /// </summary>
    public static class CacheManager
    {
        /// <summary>
        /// Default Timespan: 7 days
        /// </summary>
        private static readonly TimeSpan Expiration =  TimeSpan.FromDays(7); 
            
            
        /// <summary>
        /// Saves a value to the cache with an optional expiration.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="expiration">The expiration time. Defaults to 1 day if not specified.</param>
        /// <returns>The value cached</returns>
        public static T Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            var expiry = expiration ?? Expiration;
            return Cached<T>.Save(key, value, expiry);
        }

        /// <summary>
        /// Saves a value to the cache with an optional expiration by using 2 Keys.
        /// </summary>
        /// <param name="key1">cache key 1</param>
        /// <param name="key2">cache key 2</param>
        /// <param name="value">cache value</param>
        /// <param name="expiration">The expiration time.</param>
        /// <returns>The value cached</returns>
        public static T Set<T>(string key1, string key2, T value, TimeSpan? expiration = null)
        {
            var expiry = expiration ?? Expiration;
            return Cached<T>.Save(key1, key2, value, expiry);
        }

        /// <summary>
        /// Save a range of k-v in one operation.
        /// </summary>
        /// <param name="range">k-v pair list</param>
        /// <param name="expiration">The expiration time.</param>
        /// <typeparam name="TS">Key type</typeparam>
        /// <typeparam name="T"></typeparam>
        public static void SetRange<TS, T>(IEnumerable<(TS, T)> range, TimeSpan? expiration = null) where TS : notnull
        {
            var expiry = expiration ?? Expiration;
            CachedRange<T>.Save(range, expiry);
        }

        /// <summary>
        /// Retrieves a value from the cache.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached value, or default(T) if not found.</returns>
        public static T? Get<T>(string key)
        {
            return Cached<T>.TryGet(key, out var cachedValue) ? cachedValue.Value : default;
        }

        public static T? Get<T>(string key1, string key2)
        {
            return Cached<T>.TryGet(key1, key2, out var cachedValue) ? cachedValue.Value : default;
        }


        /// <summary>
        /// Tries to retrieve a value from the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>True if method get value of given key</returns>
        public static bool TryGet<T>(string key, out T? value)
        {
            if (Cached<T>.TryGet(key, out var cached))
            {
                value = cached.Value;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGet<T>(string key1, string key2, out T? value)
        {
            if (Cached<T>.TryGet(key1, key2, out var cached))
            {
                value = cached.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Remove cache <br/>
        /// Can only remove item with multiple keys...
        /// </summary>
        /// <param name="key1">key1</param>
        /// <param name="key2">key2</param>
        /// <typeparam name="T">value type</typeparam>
        public static void Remove<T>(string key1, string key2)
        {
            var list = new List<(string, string)> { (key1, key2) };
            CachedRange<T>.Remove(list);
        }
    }


    /// <summary>
    /// File cache helper class for manipulating channel(s) and it's photos.
    /// <br/>
    /// This file cache could be treated as original source data.
    /// </summary>
    public static class FileCacheManager
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(FileCacheManager));


        /// <summary>
        /// Save channel info to local disk cache.<br/>
        /// <para>Saved file path: AppDataFolder/AppName/channel/channels.json</para>
        /// </summary>
        /// <param name="channels">List of <see cref="UnsplashChannel"/>channels</param>
        public static async Task CacheChannelsAsync(List<UnsplashChannel> channels)
        {
            try
            {
                await File.WriteAllTextAsync(
                    FileUtils.CachePath(FileUtils.CachedPhotoBaseFolder, CachedChannelNamePrefix),
                    JsonConvert.SerializeObject(channels, JsonHelper.Settings));
            }
            catch (Exception e)
            {
                Logger.Error(e, "CacheChannelsAsync error");
            }
        }

        /// <summary>
        /// Load channels from cache.<br/>
        /// May return empty list or null
        /// </summary>
        /// <returns>Cached channels or null if exception occured</returns>
        public static async Task<List<UnsplashChannel>?> LoadChannelsAsync()
        {
            try
            {
                var filePath = FileUtils.CachePath(FileUtils.CachedPhotoBaseFolder, CachedChannelNamePrefix);
                if (!File.Exists(filePath)) return null;
                var channelString = await File.ReadAllTextAsync(filePath);

                var channels = JsonConvert.DeserializeObject<List<UnsplashChannel>>(
                    channelString,
                    JsonHelper.Settings) ?? [];
                return channels;
            }
            catch (Exception e)
            {
                Logger.Error(e, "LoadChannelsAsync error");
                return null;
            }
        }


        /// <summary>
        /// Cache Photos of specify channel which are get from unsplash pagination web API to local disk file. <br/>
        /// Full Path: AppDataFolder/AppName/photos/channelId/filename <br/>
        /// Caches are saved by shard(page).
        /// </summary>
        /// <returns></returns>
        public static async Task CachePhotosAsync(PhotosCachePageIndex index, List<UnsplashPhoto> photos)
        {
            try
            {
                var dir = FileUtils.CreateDir(FileUtils.CachedPhotoBaseFolder, index.ChannelId);
                var filePath = FileUtils.CachePath(dir, $"{CachedPhotosNamePrefix}{index.PageIndex}");
                await File.WriteAllTextAsync(filePath,
                    JsonConvert.SerializeObject(photos, JsonHelper.Settings));
            }
            catch (Exception e)
            {
                Logger.Error(e, "CachePhotosAsync error");
            }
            // Console.WriteLine($@"Saved {photos.Count} photos to cache");
        }

        /// <summary>
        /// Get specify shard of channel's photo list from cache file.<br/>
        /// </summary>
        /// <param name="index"><see cref="PhotosCachePageIndex"/>></param>
        /// <returns><see cref="UnsplashPhoto"/> list, or null if cache file does not exist or exception occurs</returns>
        public static async Task<List<UnsplashPhoto>?> LoadPhotosShardAsync(PhotosCachePageIndex index)
        {
            var filePath = FileUtils.CachePath(
                Path.Combine(FileUtils.CachedPhotoBaseFolder, index.ChannelId),
                $"{CachedPhotosNamePrefix}{index.PageIndex}");
            if (!File.Exists(filePath)) return null;
            try
            {
                var photoString = await File.ReadAllTextAsync(filePath);

                var photos = JsonConvert.DeserializeObject<List<UnsplashPhoto>>(
                    photoString,
                    JsonHelper.Settings) ?? [];
                return photos;
            }
            catch (Exception e)
            {
                Logger.Error(e, "LoadPhotosShardAsync error");
                return null;
            }
        }

        /// <summary>
        ///  Load all cached photos count of specified channel by channelID.
        /// </summary>
        /// <param name="channelId">channelID</param>
        /// <returns>Cached photo count.</returns>
        [Obsolete("This result is not accurate once filter is on.")]
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
                $"{CachedPhotosNamePrefix}{maxShard}.{CachedFileNameSuffix}");
            var json = await File.ReadAllTextAsync(latestShard);
            var latestShardCount =
                JsonConvert.DeserializeObject<List<UnsplashPhoto>>(json, JsonHelper.Settings)
                    ?.Count;

            return (int)((files.Length - 1) * PageSize + latestShardCount)!;
        }

        /// <summary>
        /// Delete channel and it's photos
        /// </summary>
        /// <param name="channelId"></param>
        public static void UnCacheChannelPhotos(string channelId)
        {
            // Remove disk caches
            var dir = Path.Combine(FileUtils.CachedPhotoBaseFolder, channelId);
            FileUtils.DeleteFolder(dir);
        }


        /// <summary>
        /// Cache wallpaper sequence of each channel.
        /// </summary>
        /// <param name="cachedWallpaperSequence"></param>
        /// <returns></returns>
        public static async Task CacheChannelSequence(Dictionary<string, int> cachedWallpaperSequence)
        {
            var filePath = FileUtils.CachePath(FileUtils.CachedPhotoBaseFolder, CachedChannelSeqPrefix);
            try
            {
                await File.WriteAllTextAsync(filePath,
                    JsonConvert.SerializeObject(cachedWallpaperSequence, JsonHelper.Settings));
            }
            catch (Exception e)
            {
                Logger.Error(e, "ChannelSequence error");
                throw;
            }
        }

        /// <summary>
        /// Load all channels wallpaper sequence.
        /// </summary>
        /// <returns>Channel sequence dictionary, or null if exception occurs</returns>
        public static async Task<Dictionary<string, int>?> LoadChannelSequence()
        {
            var filePath = FileUtils.CachePath(FileUtils.CachedPhotoBaseFolder, CachedChannelSeqPrefix);
            try
            {
                if (!File.Exists(filePath)) return null;
                var sequence = await File.ReadAllTextAsync(filePath);
                var cachedSequence = JsonConvert.DeserializeObject<Dictionary<string, int>>(
                    sequence,
                    JsonHelper.Settings) ?? new Dictionary<string, int>();
                return cachedSequence;
            }
            catch (Exception e)
            {
                Logger.Error(e, "LoadChannelSequence error");
                throw;
            }
            // Console.WriteLine($@"Loaded {cachedSequence.Count} sequence from cache");
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