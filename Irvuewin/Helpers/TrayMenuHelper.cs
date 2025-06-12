using System.Diagnostics;
using Irvuewin.Helpers.Utils;
using Irvuewin.Models;
using Irvuewin.Models.Unsplash;
using Irvuewin.ViewModels;

namespace Irvuewin.Helpers;

///<summary>
///Author: wangy325
///Date: 2025-06-06 10:04:10
///Desc: Wallpaper operation class
///</summary>
public static class TrayMenuHelper
{
    private static Stack<string> _wallpaperHistory = new(10);

    private static string _currentScreen;

    private static int _sequenceModify;

    private static readonly Dictionary<string, int> CachedWallpaperSequence = new();

    // screen name, screen wallpaper stack
    private static Dictionary<string, Stack<string>> _wallpaperStack = new();
    private static readonly Random Random = new();

    public static async void LoadCachedSequence()
    {
        // TODO NOT PERFECT
        if (CachedWallpaperSequence.Count > 0) return;
        var sequence = await UnsplashCache.LoadChannelSequence();
        if (sequence is not null && sequence.Count != 0)
        {
            foreach (var pair in sequence)
            {
                CachedWallpaperSequence[pair.Key] = pair.Value;
            }
        }
        else
        {
            foreach (var channel in UnsplashCache.CachedChannels)
            {
                CachedWallpaperSequence[channel.Id] = 1;
            }
        }

        Console.WriteLine($@">>> Load Cached wallpaper sequence: {CachedWallpaperSequence}");
    }

    public static async void SaveCachedSequence()
    {
        if (_sequenceModify <= 0) return;
        await UnsplashCache.SaveChannelSequence(CachedWallpaperSequence);
        Console.WriteLine($@">>> Save Cached wallpaper sequence: {CachedWallpaperSequence}");
        // reset
        _sequenceModify = 0;
    }

    public static void ChangeCurrentWallpaper(UnsplashChannel channel)
    {
        var randomWallpaper = Properties.Settings.Default.RandomWallpaper;
        if (randomWallpaper)
        {
            var total = channel.TotalPhotos;
            var random = Random.Next(1, total);
            // var random = 10;
            SetWallPaper(channel, random, true);
        }
        else
        {
            // 软件启动时 （不主动更改壁纸）
            // 是否需要缓存栈？ 暂时不需要，启动时空栈即可
            // TODO 多显示器的栈缓存还不一样呢
            var sequence = CachedWallpaperSequence[channel.Id];
            SetWallPaper(channel, sequence);
            // TODO 注意边界异常
            if (++sequence > channel.TotalPhotos)
            {
                sequence %= channel.TotalPhotos;
            }

            _sequenceModify++;
            CachedWallpaperSequence[channel.Id] = sequence;
        }
    }


    private static async void SetWallPaper(UnsplashChannel channel, int sequence, bool random = false)
    {
        var (shardIndex, shardPositionIndex) = CalShardIndex(sequence);

        PhotosCachePageIndex photosCachePageIndex = new()
        {
            ChannelId = channel.Id,
            PageIndex = shardIndex
        };

        // var res = await UnsplashCache.LoadPhotosAsync(photosCache);
        string? path;
        if (UnsplashCache.CachedPhotos.TryGetValue(photosCachePageIndex, out var res))
        {
            Console.WriteLine(@">>> Set wallpaper from cache.");
            path = await WallpaperUtil.SetWallpaper(res[shardPositionIndex]);
        }
        else
        {
            Console.WriteLine(@">>> Set wallpaper from web.");
            const int pageSize = 10;
            var httpService = new UnsplashHttpService(new UnsplashHttpClientWrapper());
            UnsplashPhoto? photo;
            if (random)
            {
                photo = await httpService.GetRandomPhotoInChannel(channel.Id);
            }
            else
            {
                photo = null;
                var query = new UnsplashQueryParams()
                {
                    Page = shardIndex,
                    PerPage = pageSize,
                    Orientation = Properties.Settings.Default.WallpaperOrientation
                };
                if (await httpService.GetPhotosOfChannel(channel.Id, query) is { } photos
                    && photos.Count != 0)
                {
                    // update cache
                    await UnsplashCache.SavePhotosAsync(photosCachePageIndex, photos);
                    photo = photos[shardPositionIndex];
                }
            }

            // TODO null pointer?
            Debug.Assert(photo != null, nameof(photo) + " != null");
            path = await WallpaperUtil.SetWallpaper(photo);
        }

        // Push photo file into wallpaper history stack
        if (path is not null)
        {
            _wallpaperHistory.Push(path);
            _wallpaperStack[_currentScreen] = _wallpaperHistory;
        }
    }

    private static (int shardIndex, int shardPositionIndex) CalShardIndex(int sequence)
    {
        const int pageSize = 10;
        int shardIndex, shardPositionIndex;
        // Index starts from 1
        if (sequence % pageSize == 0)
        {
            shardIndex = sequence / pageSize;
            shardPositionIndex = pageSize - 1;
        }
        else
        {
            shardIndex = sequence / pageSize + 1;
            shardPositionIndex = sequence % pageSize - 1;
        }

        return (shardIndex, shardPositionIndex);
    }

    // 检查指针在哪个屏幕
    public static void CheckPointer(object sender)
    {
        _currentScreen = DisplayInfoHelper.CheckDisplay().name;
    }

    public static void ChangeAllWallpaper(ChannelViewModel channel)
    {
        // TODO change all wallpaper
    }


    public static void WallpaperInfo(ChannelViewModel selectedChannel)
    {
        // TODO 多显示器时注意
        // sequence
        var sequence = --CachedWallpaperSequence[selectedChannel.Id];
        var (shardIndex, shardPositionIndex) = CalShardIndex(sequence);
        PhotosCachePageIndex photosCachePageIndex = new()
        {
            ChannelId = selectedChannel.Id,
            PageIndex = shardIndex
        };
        // Key must exist
        // UnsplashCache.CachedPhotos.TryGetValue(photosCachePageIndex, out var res);
        var res = UnsplashCache.CachedPhotos[photosCachePageIndex];
        var photo = res[shardPositionIndex];
    }

    public static async void PreviousWallpaper()
    {
        // TODO 多显示器时注意
        // Do nothing when stack is empty or stack has only current wallpaper
        // This happens when app starts up
        if (_wallpaperHistory.Count <= 1) return;
        _wallpaperHistory.Pop();
        var path = _wallpaperHistory.Peek();
        await WallpaperUtil.SetWallpaper(null, path);
    }

    public static bool DownloadCurrentWallpaper(string dest)
    {
        // TODO 多显示器时注意
        if (_wallpaperHistory.Count < 1) return false;
        var path = _wallpaperHistory.Peek();
        FileUtils.CopyFileToDir(path, dest);
        return true;
    }
}