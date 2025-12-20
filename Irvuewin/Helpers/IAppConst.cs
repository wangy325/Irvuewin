using static System.Environment;
using static System.Environment.SpecialFolder;

namespace Irvuewin.Helpers;

public interface IAppConst
{
    const int PageSize = 12;
    
    static readonly string DefaultWallpaperDownloadDir = GetFolderPath(MyPictures);
    
    const string AppName = "Irvuewin";
    // const string WallpaperCacheFolder = "splash";
    // const string ChannelCacheFolder = "channel";

    const string CachedWindowsPositionFileSuffix = "xml";
    const string CachedFileNameSuffix = "json";
    const string CachedChannelNamePrefix = "channels";
    const string CachedPhotosNamePrefix = "photos_";
    const string CachedChannelSequenceNamePrefix =  "sequence";
    
    
    
    // Errors:
    const string InMemoryCacheError = "InMemoryCacheError";
    const string FileCacheError = "FileCacheError";
}