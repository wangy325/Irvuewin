using System.Diagnostics;
using System.IO;
using Irvuewin.Helpers.Utils;
using Irvuewin.Models;
using Irvuewin.Models.Unsplash;

namespace Irvuewin.Helpers
{
    ///<summary>
    ///Author: wangy325
    ///Date: 2025-06-06 10:04:10
    ///Desc: Wallpaper operation class
    ///</summary>
    public class TrayMenuHelper
    {
        private Stack<UnsplashPhoto> _wallpaperHistory = new(10);

        private string _currentScreen;

        // channelId
        private Dictionary<string, Stack<UnsplashPhoto>> _wallpaperStack = new();

        private static readonly Random Random = new();


        // 非随机壁纸的情况下需要知道
        // 栈的大小
        // 知道page index

        // 软件启动时 （不主动更改壁纸）
        // 是否需要缓存栈？

        // 多显示器的栈缓存还不一样呢

        public static async void ChangeCurrentWallpaper(UnsplashChannel channel)
        {
            var randomWallpaper = Properties.Settings.Default.RandomWallpaper;
            if (randomWallpaper)
            {
                // TODO: Is there a way to get photo from cache?
                var total = channel.TotalPhotos;
                var random = Random.Next(0, total);
                // index start from 1
                var shardIndex = random / 10 + 1;

                Debug.WriteLine($"Random: {random}");
                Debug.WriteLine($"Shard index: {shardIndex}");
                // var cachedPhotos = $"photos_{channel.Id}_{shardIndex}.cached.json";
                PhotosCachePageIndex photosCachePageIndex = new()
                {
                    ChannelId = channel.Id,
                    PageIndex = shardIndex
                };
                var res = await UnsplashCache.LoadPhotosAsync(photosCachePageIndex);
                if (res is not null)
                {
                    var shardPositionIndex = random % 10;
                    WallpaperUtil.SetWallpaper(res[shardPositionIndex - 1]);
                    Debug.WriteLine($"Set wallpaper from cache.");
                }
                else
                {
                    var httpService = new UnsplashHttpService(new UnsplashHttpClientWrapper());
                    if (await httpService.GetRandomPhotoInChannel(channel.Id) is not { } photo) return;
                    WallpaperUtil.SetWallpaper(photo);
                    Debug.WriteLine($"Set wallpaper from web.");
                }
            }
            else
            {
                // TODO 顺序切换壁纸
            }
        }
    }
}