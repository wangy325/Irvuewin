using Irvuewin.Models.Unsplash;
using Serilog;

namespace Irvuewin.Helpers.DB;

public class DataBaseService
{
    private static readonly ILogger Logger = Log.ForContext<DataBaseService>();


    /// <summary>
    /// Get channel by id
    /// </summary>
    /// <param name="cid"></param>
    /// <returns>null if no channel find.</returns>
    public static UnsplashChannel? GetChannel(string cid)
    {
        return DatabaseManager.GetChannelById(cid);
    }

    /// <summary>
    /// Async update channel (sequence) info 
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    public static Task UpdateChannel(UnsplashChannel channel)
    {
        return Task.Run(() => DatabaseManager.UpsertChannel(channel));
    }

    /// <summary>
    /// Save channel info to LiteDB.<br/>
    /// <para>Saved path: AppDataFolder/AppName/channel/</para>
    /// </summary>
    /// <param name="channels">List of <see cref="UnsplashChannel"/>channels</param>
    public static Task CacheChannels(List<UnsplashChannel> channels)
    {
        return Task.Run(() => DatabaseManager.UpsertChannel(channels));
    }

    /// <summary>
    /// Load channels from LiteDB.<br/>
    /// May return empty list or null
    /// </summary>
    /// <returns>Cached channels or null if exception occured</returns>
    public static List<UnsplashChannel>? LoadChannels()
    {
        return DatabaseManager.GetAllChannels();
    }


    /// <summary>
    /// Cache Photos of specify channel which are gotten from unsplash pagination web API to LiteDB. <br/>
    /// </summary>
    /// <returns></returns>
    public static Task CachePhotos(string channelId, List<UnsplashPhoto> photos)
    {
        return Task.Run(() =>
        {
            try
            {
                DatabaseManager.UpsertPhotos(channelId, photos);
            }
            catch (Exception e)
            {
                Logger.Error(e, "CachePhotosAsync error");
            }
        });
    }

    /// <summary>
    /// Get specify page of channel's photo list from LiteDB.<br/>
    /// </summary>
    /// <param name="cid">channelId</param>
    /// <param name="index"><see cref="UnsplashQueryParams"/>>share pagination fields</param>
    /// <returns><see cref="UnsplashPhoto"/> list, or null if cache file does not exist or exception occurs</returns>
    public static Task<List<UnsplashPhoto>> LoadPhotosShardAsync(string cid, UnsplashQueryParams index)
    {
        return Task.Run(() =>
        {
            try
            {
                return DatabaseManager.GetPhotos(cid, index.Page, index.PerPage);
            }
            catch (Exception e)
            {
                Logger.Error(e, "LoadPhotosShardAsync error");
                return [];
            }
        });
    }

    /// <summary>
    ///  Load all cached photos count of specified channel by channelID. Including filtered photos.
    /// </summary>
    /// <param name="channelId">channelID</param>
    /// <returns>Cached photo count.</returns>
    public static Task<int> LoadPhotoCount(string channelId)
    {
        return Task.Run(() => DatabaseManager.CountPhotos(channelId));
    }

    /// <summary>
    /// Load all cached photos of specify chanel id.
    /// </summary>
    /// <param name="cid">channel id</param>
    /// <returns>List of photos, or null if no photo cached or exception occured.</returns>
    public static Task<List<UnsplashPhoto>?> LoadPhotosAsync(string cid)
    {
        return Task.Run(() =>
        {
            try
            {
                var photos = DatabaseManager.GetPhotos(cid);
                return photos.Count > 0 ? photos : null;
            }
            catch (Exception e)
            {
                Logger.Error(e, "LoadPhotosAsync error");
                return null;
            }
        });
    }

    public static Task UpdatePhoto(UnsplashPhoto photo)
    {
        return Task.Run(() => DatabaseManager.UpsertPhoto(photo));
    }

    /// <summary>
    /// Delete channel and it's photos
    /// </summary>
    /// <param name="channelId"></param>
    public static void RemoveChannel(string channelId)
    {
        Task.Run(() =>
        {
            DatabaseManager.RemoveChannel(channelId);
            DatabaseManager.RemoveChannelPhotos(channelId);
        });
    }

    public static Task RemoveChannelPhotos(string channelId)
    {
        return Task.Run(() => DatabaseManager.RemoveChannelPhotos(channelId));
    }
}