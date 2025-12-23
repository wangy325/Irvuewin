using static System.Environment;
using static System.Environment.SpecialFolder;

namespace Irvuewin.Helpers;

public interface IAppConst
{
    const int PageSize = 3;

    static readonly string DefaultWallpaperDownloadDir = GetFolderPath(MyPictures);

    const string AppName = "Irvuewin";
    // const string WallpaperCacheFolder = "splash";
    // const string ChannelCacheFolder = "channel";

    const string CachedWindowsPositionFileSuffix = "xml";
    const string CachedFileNameSuffix = "json";
    
    // cached channel file name prefix
    const string CachedChannelNamePrefix = "channels";

    // cached channel's photo file name prefix
    const string CachedPhotosNamePrefix = "photos_";

    // key for channel's wallpaper queue sequence / local sequence file name prefix
    const string CachedChannelSeqPrefix = "sequence";

    // key for display's wallpaper history stack
    const string CachedWallpaperStack = "wallpaper_stack";

    // key for channel's wallpaper loaded
    const string CachedWallpapers = "channel_wallpaper_count";
    
    const string CachedWallpaperShard =  "channel_wallpaper_shard";


    // Errors:
    const string InMemoryCacheError = "InMemoryCacheError";
    const string FileCacheError = "FileCacheError";
}