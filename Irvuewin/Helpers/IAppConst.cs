using static System.Environment;
using static System.Environment.SpecialFolder;

namespace Irvuewin.Helpers;

public interface IAppConst
{
    const int PageSize = 12;

    static readonly string DefaultWallpaperDownloadDir = GetFolderPath(MyPictures);

    const string AppName = "Irvuewin";

    const string WallpaperCacheFolder = "splash";
    const string ChannelCacheFolder = "channel";

    const string CachedConfigFileFormat = "xml";
    const string CachedResourceFileFormat = "json";
    


    /// <summary>
    ///  In-memory cache keys
    /// </summary>
    // key for channel's wallpaper queue sequence / local sequence file name prefix
    // const string CachedChannelSeqPrefix = "sequence";

    // key for display's wallpaper history stack
    const string CachedWallpaperStack = "wallpaper_stack";

    // key for channel's wallpaper loaded
    // const string CachedWallpapers = "channel_wallpaper_count";

    // const string CachedWallpaperShard = "channel_wallpaper_shard";
    // db-instead end


    // Errors:
    const string InMemoryCacheError = "InMemoryCacheError";
    const string FileCacheError = "FileCacheError";

    // Urls
    const string BaseApiUrl = "https://unsplash-api-proxy.wangy325.workers.dev";
    const string Photos = "photos";
    const string Collections = "collections";
    const string User = "users";
    const string Search = "search";
    const string Attribute = "?utm_source=your_app_name&utm_medium=referral";

    // LiteDB
    const string DbPhotoCollection = "photos";
    const string DbChannelCollection = "collections";
    const string DbJobsCollection = "jobs";
    const string DbFiltersCollection = "filters";
}