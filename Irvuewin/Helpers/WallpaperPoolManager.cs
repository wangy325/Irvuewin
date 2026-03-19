using Irvuewin.Helpers.DB;
using Irvuewin.Helpers.Events;
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
    private static readonly Lock _lock = new();

    // public static WallpaperPoolManager Instance => _instance ?? throw new InvalidOperationException("WallpaperPoolManager is not initialized. Call Initialize() first.");

    public static void Initialize(UnsplashHttpService apiService)
    {
        if (_instance != null) return;
        lock (_lock)
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
        if (_isFetching) return;
        _isFetching = true;
        try
        {
            var channel = DataBaseService.GetChannel(channelId)!;

            var query = new UnsplashQueryParams()
            {
                Page = ++channel.Shard,
                PerPage = PageSize,
                Orientation = Properties.Settings.Default.WallpaperOrientation
            };

            // 这里作为拉取unsplash壁纸的唯一入口
            if (await _apiService.GetPhotosOfChannel(channelId: channelId, query) is not { } photos) return;
            // break;


            // Update channel's shard and load flag if necessary
            if (photos.Count == 0)
            {
                // Sometimes api gets 0 photo from channel
                // Though channel contains photo(s)
                // We assume that all photos are loaded
                channel.AllPhotosLoaded = true;
                // await DataBaseService.UpdateChannel(channel);
                // break;
            }
            else
            {
                await DataBaseService.CachePhotos(channelId, photos);
            }

            // update channel after all
            await DataBaseService.UpdateChannel(channel);
            EventBus.PublishWallpapersReplenished();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Fetch wallpapers failed.");
        }
        finally
        {
            _isFetching = false;
        }
    }

    // 水位线检测
    // 什么时候会触发？触发几次？
    // 好像只要启动时触发一次就行
    private void CheckWatermarkAsync()
    {
        var channels = DataBaseService.LoadChannels();
        if (channels is not { Count: > 0 }) return;
        foreach (var channel in channels)
        {
            while (DataBaseService.LoadedPhotosCountExcluded(channel.Id) < PhotoPoolWaterMark)
            {
                FetchMoreWallpapersIfNeeded(channel.Id);
            }
        }
    }
}