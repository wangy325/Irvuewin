using System.IO;
using Irvuewin.Helpers;
using Irvuewin.Helpers.Utils;
using Irvuewin.Models.Unsplash;
using Newtonsoft.Json;
using Serilog;

namespace Irvuewin.Models
{
    using static IAppConst;

    /// <summary>
    /// File cache helper class for manipulating channel(s) and it's photos.
    /// <br/>
    /// This file cache could be treated as original source data.
    /// </summary>
    public static class UnsplashCache
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(UnsplashCache));


        /// <summary>
        /// Save channel info to local disk cache.<br/>
        /// <para>Saved file path: AppDataFolder/AppName/channel/channels.json</para>
        /// </summary>
        /// <param name="channels">List of <see cref="UnsplashChannel"/>channels</param>
        public static async Task CacheChannelsAsync(List<UnsplashChannel> channels)
        {
            try
            {
                await File.WriteAllTextAsync(FileUtils.CachePath(FileUtils.CachedPhotoBaseFolder, CachedChannelNamePrefix),
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
            var filePath = FileUtils.CachePath(FileUtils.CachedPhotoBaseFolder, CachedChannelSequenceNamePrefix);
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
        /// <returns></returns>
        public static async Task<Dictionary<string, int>?> LoadChannelSequence()
        {
            var filePath = FileUtils.CachePath(FileUtils.CachedPhotoBaseFolder, CachedChannelSequenceNamePrefix);
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
                Logger.Error(e, "LoadChannelSequence error");
                throw;
            }
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