using Serilog;

namespace Irvuewin.Helpers.Events;

public static class EventBus
{
    private static readonly ILogger Logger = Log.ForContext(typeof(EventBus));

    // Wallpaper changed event
    public class WallpaperChangedEventArgs(string displayName, string photoId) : EventArgs
    {
        public string DisplayName { get; } = displayName;
        public string PhotoId { get; } = photoId;
    }

    public static event EventHandler<WallpaperChangedEventArgs>? WallpaperChangedEvent;

    public static void PublishWallpaperChanged(string displayName, string photoId)
    {
        Logger.Information("Publishing wallpaper changed event for display: {}", displayName);
        WallpaperChangedEvent?.Invoke(null, new WallpaperChangedEventArgs(displayName, photoId));
    }

    // Wallpaper pool - background pre-fetch
    public static event Action<string>? PoolLowRequested; // load more photos from unsplash
    public static event Action? WallpapersReplenished; // photos are ready to display

    public static void PublishPoolLow(string cid)
    {
        Logger.Information("Publishing pool low event for cid {cid}", cid);
        PoolLowRequested?.Invoke(cid);
    }

    public static void PublishWallpapersReplenished()
    {
        Logger.Information("Publishing wallpapers replenished event");
        WallpapersReplenished?.Invoke();
    }
}