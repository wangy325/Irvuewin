using static System.Environment;
using static System.Environment.SpecialFolder;

namespace Irvuewin.Helpers;

public interface IAppConst
{
    const int PageSize = 12;
    const string LikesChannelId = "likes";

    static readonly string DefaultWallpaperDownloadDir = GetFolderPath(MyPictures);


    const string AppName = "Irvuewin";

    const string WallpaperCacheFolder = "splash";
    const string ChannelCacheFolder = "channel";

    const string CachedConfigFileFormat = "xml";
    const string CachedResourceFileFormat = "json";
    


    /// <summary>
    ///  In-memory cache keys
    /// </summary>
    // key for display's wallpaper history stack
    const string CachedWallpaperStack = "wallpaper_stack";
    // key for channels wallpaper preview gallery
    const string CachedWallpaperPreviewShard = "wallpaper_preview_shard";


    // Errors:
    const string InMemoryCacheError = "InMemoryCacheError";
    const string FileCacheError = "FileCacheError";

    // Urls
    const string BaseApiUrl = "https://unsplash-api-proxy.wangy325.workers.dev";
    const string OriginImageUrl = "https://images.unsplash.com";
    const string ImageProxyUrl = "https://unsplash-image-proxy.wangy325.workers.dev";
    const string Photos = "photos";
    const string Collections = "collections";
    const string User = "users";
    const string Search = "search";
    const string Attribution = "?utm_source=MagicPaper&utm_medium=referral";

    // GitHub
    const string GitHubRepoUrl = "https://github.com/wangy325/Irvuewin";
    const string GitHubIssuesUrl = "https://github.com/wangy325/Irvuewin/issues";
    const string GitHubReleasesUrl = "https://github.com/wangy325/Irvuewin/releases";
    const string GitHubLatestReleaseApi = "https://api.github.com/repos/wangy325/Irvuewin/releases/latest";
    const string GitHubLicenseUrl = "https://github.com/wangy325/Irvuewin/blob/main/LICENSE.txt";

    // LiteDB
    const string DbPhotoCollection = "photos";
    const string DbChannelCollection = "collections";
    const string DbJobsCollection = "jobs";
    const string DbFiltersCollection = "filter";
    
    // body keywords
    static readonly string[] PhotoFilterWords = ["woman", "women", "man", "men", "people", "boy", "girl", "baby", "person"];
    
    // watermark 
    const int PhotoPoolWaterMark = 20;
}