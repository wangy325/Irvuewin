using Irvuewin.Helpers.DB;
using Irvuewin.Helpers.Events;
using Irvuewin.Helpers.HTTP;
using Irvuewin.Models.Unsplash;
using Serilog;
using static Irvuewin.Helpers.IAppConst;

namespace Irvuewin.Helpers;

public class WallpaperPoolManager
{
    private static readonly ILogger Logger = Log.ForContext<WallpaperPoolManager>();

    private readonly UnsplashHttpService _apiService;

    private bool _isFetching;

    private static WallpaperPoolManager? _instance;
    private static readonly Lock Lock = new();

    public static WallpaperPoolManager Instance => _instance ??
                                                   throw new InvalidOperationException(
                                                       "WallpaperPoolManager is not initialized. Call Initialize() first.");

    public static void Initialize(UnsplashHttpService apiService)
    {
        if (_instance != null) return;
        lock (Lock)
        {
            _instance ??= new WallpaperPoolManager(apiService);
        }

        Logger.Information("Wallpaper pool manager is initialized.");
    }

    private WallpaperPoolManager(UnsplashHttpService apiService)
    {
        _apiService = apiService;
        EventBus.PoolLowRequested += FetchMoreWallpapersIfNeeded;
        // 启动时的水位线检查
        CheckWatermarkAsync();
    }

    private async void FetchMoreWallpapersIfNeeded(string channelId)
    {
        await FetchMoreWallpapersInternalAsync(channelId);
    }

    /// <summary>
    /// Explicitly fetch wallpapers and await the operation status.
    /// </summary>
    public Task<bool> FetchWallpapersAsync(string channelId)
    {
        return FetchMoreWallpapersInternalAsync(channelId);
    }

    /// <summary>
    /// Only entrance of pulling unsplash photos.
    /// </summary>
    /// <param name="channelId"></param>
    /// <returns></returns>
    private async Task<bool> FetchMoreWallpapersInternalAsync(string channelId)
    {
        if (_isFetching) return false;
        _isFetching = true;
        try
        {
            var channel = DataBaseService.GetChannel(channelId)!;
            var poolSize = DataBaseService.LoadedPhotosCountExcluded(channelId);
            if (poolSize >= MaxPhotoPoolSize)
            {
                // assume all photos loaded
                channel.AllPhotosLoaded = true;
                return false; 
            }

            // var query = UnsplashQueryParams.Create().Page(channel.Shard);
            // if (await _apiService.GetPhotosOfChannel(channelId: channelId, query) is not { } photos) return false;
            
            // update v 1.0.10: get random wallpaper  
            if (await _apiService.GetRandomPhotoInChannel(channelId, PageSize) is not { } photos) return false;
            
            // Update channel's shard and load flag if necessary
            if (photos.Count == 0)
            {
                // Sometimes api gets 0 photo from channel
                // Though channel contains photo(s)
                // We assume that all photos are loaded
                channel.AllPhotosLoaded = true;
                await DataBaseService.UpdateChannel(channel);
                return false;
            }
            else
            {
                // 只有真正获取到了数据才递增分片翻页
                channel.Shard++; 
                await DataBaseService.CachePhotos(channelId, photos);
                EventBus.PublishWallpapersReplenished();
                await DataBaseService.UpdateChannel(channel);
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Fetch wallpapers failed.");
            return false;
        }
        finally
        {
            _isFetching = false;
        }
    }

    // 水位线检测
    // 什么时候会触发？触发几次？
    // 好像只要启动时触发一次就行
    private async void CheckWatermarkAsync()
    {
        var channels = DataBaseService.LoadChannels();
        if (channels is not { Count: > 0 }) return;
        foreach (var channel in channels)
        {
            var maxAttempts = 5;
            while (DataBaseService.LoadedPhotosCountExcluded(channel.Id) < PhotoPoolWaterMark && maxAttempts > 0)
            {
                var currentChannel = DataBaseService.GetChannel(channel.Id);
                if (currentChannel == null || currentChannel.AllPhotosLoaded) break;

                var success = await FetchMoreWallpapersInternalAsync(channel.Id);
                if (!success) break;

                maxAttempts--;
            }
        }

        Logger.Information("Water marker Check.");
    }
}