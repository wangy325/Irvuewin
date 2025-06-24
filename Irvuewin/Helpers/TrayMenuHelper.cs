using System.Diagnostics;
using System.Windows;
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
    private static readonly Stack<string> WallpaperHistory = new(10);

    private static int _sequenceModify;

    // Wallpaper sequence for each channel (key is channelId)
    // Used to locate wallpaper in sequence mode
    private static readonly Dictionary<string, int> CachedWallpaperSequence = new();

    // screen name, screen wallpaper stack
    private static readonly Dictionary<string, Stack<string>> WallpaperStack = new();

    private static string? _currentScreen;

    // Current wallpaper of each screen (key is screen id, value is photoId)
    private static readonly Dictionary<string, string> CurrentWallpaper = new();

    private static readonly Random Random = new();

    // Once new channel added, add its sequence to cache
    public static void AddNewChannelSequence(List<UnsplashChannel> channels)
    {
        foreach (var channel in channels)
        {
            CachedWallpaperSequence[channel.Id] = 1;
        }

        _sequenceModify++;
    }

    public static void DelChannelSequence(string key)
    {
        CachedWallpaperSequence.Remove(key);
        _sequenceModify++;
    }

    public static async Task LoadCachedSequence()
    {
        // TODO NOT PERFECT
        if (CachedWallpaperSequence.Count > 0) return;
        var sequence = await UnsplashCache.LoadChannelSequence();
        if (sequence is not null && sequence.Count != 0)
            // Load from disk cache
        {
            foreach (var pair in sequence)
            {
                CachedWallpaperSequence[pair.Key] = pair.Value;
            }
        }
        else
            // Load from cached channels
        {
            // TODO null check
            var channels = Properties.Settings.Default.UserUnsplashChannels.Split(",");
            foreach (var id in channels)
            {
                CachedWallpaperSequence[id] = 1;
            }
        }

        // Console.WriteLine($@">>> Load Cached wallpaper sequence: {CachedWallpaperSequence.Count}");
    }

    public static async void SaveCachedSequence()
    {
        if (_sequenceModify <= 0) return;
        await UnsplashCache.CacheChannelSequence(CachedWallpaperSequence);
        // Console.WriteLine($@">>> Save Cached wallpaper sequence: {CachedWallpaperSequence}");
        // reset
        _sequenceModify = 0;
    }

    public static async Task ChangeCurrentWallpaper(UnsplashChannel channel)
    {
        var randomWallpaper = Properties.Settings.Default.RandomWallpaper;
        if (randomWallpaper)
        {
            var total = channel.TotalPhotos;
            var random = Random.Next(1, total);
            // var random = 10;
            await SetWallPaper(channel, random, true);
        }
        else
        {
            // 软件启动时 （不主动更改壁纸）
            // 是否需要缓存栈？ 暂时不需要，启动时空栈即可
            // TODO 多显示器的栈缓存还不一样呢
            var sequence = CachedWallpaperSequence[channel.Id];
            await SetWallPaper(channel, sequence);
            // TODO 注意边界异常
            if (++sequence > channel.TotalPhotos)
            {
                sequence %= channel.TotalPhotos;
            }

            _sequenceModify++;
            CachedWallpaperSequence[channel.Id] = sequence;
        }
    }


    private static async Task SetWallPaper(UnsplashChannel channel, int sequence, bool random = false)
    {
        var (shardIndex, shardPositionIndex) = CalShardIndex(sequence);

        PhotosCachePageIndex photosCachePageIndex = new()
        {
            ChannelId = channel.Id,
            PageIndex = shardIndex
        };

        // _currentScreen is null once app started
        CheckPointer();
        
        // var res = await UnsplashCache.LoadPhotosAsync(photosCache);
        string? path;
        UnsplashPhoto? photo;
        if (UnsplashCache.CachedPhotos.TryGetValue(photosCachePageIndex, out var res))
        {
            // Console.WriteLine(@">>> Set wallpaper from cache.");
            photo = res[shardPositionIndex];
            path = await WallpaperUtil.SetWallpaper(photo);
        }
        else
        {
            // Console.WriteLine(@">>> Set wallpaper from web.");
            const int pageSize = 10;
            var httpService = new UnsplashHttpService(new UnsplashHttpClientWrapper());
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
                    await UnsplashCache.CachePhotosAsync(photosCachePageIndex, photos);
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
            WallpaperHistory.Push(path);
            CurrentWallpaper[_currentScreen!] = photo.Id;
            WallpaperStack[_currentScreen!] = WallpaperHistory;
            await GetWallpaperInfo();
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
    public static void CheckPointer()
    {
        _currentScreen = DisplayInfoHelper.CheckDisplay().name;
    }

    public static void ChangeAllWallpaper(ChannelViewModel channel)
    {
        // TODO change all wallpaper
    }


    public static async Task GetWallpaperInfo()
    {
        // TODO 多显示器时注意
        /*var sequence = --CachedWallpaperSequence[selectedChannel.Id];
        var (shardIndex, shardPositionIndex) = CalShardIndex(sequence);
        PhotosCachePageIndex photosCachePageIndex = new()
        {
            ChannelId = selectedChannel.Id,
            PageIndex = shardIndex
        };
        // Key must exist
        // UnsplashCache.CachedPhotos.TryGetValue(photosCachePageIndex, out var res);
        var res = UnsplashCache.CachedPhotos[photosCachePageIndex];
        var photo = res[shardPositionIndex];*/
        if (_currentScreen is null)
        {
            CheckPointer();
        }

        if (!CurrentWallpaper.TryGetValue(_currentScreen!, out var photoId)) return;
        // Photos cached by collectionId do not contain downloads info 
        var httpService = IHttpClient.GetUnsplashHttpService();
        if (await httpService.GetPhotoInfoById(photoId) is { } photo)
        {
            var trayViewModel = Application.Current.Resources["TrayViewModel"] as TrayViewModel;
            
            trayViewModel!.AboutWallpaper.WallpaperLikes = (int)photo.Likes;
            trayViewModel.AboutWallpaper.WallpaperDownloads = (int)photo.Downloads;
            trayViewModel.AboutWallpaper.WallpaperAuthor = photo.User.Name;
            trayViewModel.AboutWallpaper.AuthorProfile = photo.User.Links.Html.OriginalString;
        }
    }

    public static async void PreviousWallpaper()
    {
        // TODO 多显示器时注意
        // Do nothing when stack is empty or stack has only current wallpaper
        // This happens when app starts up
        if (WallpaperHistory.Count <= 1) return;
        WallpaperHistory.Pop();
        var path = WallpaperHistory.Peek();
        await WallpaperUtil.SetWallpaper(null, path);
    }

    public static bool DownloadCurrentWallpaper(string dest)
    {
        // TODO 多显示器时注意
        if (WallpaperHistory.Count < 1) return false;
        var path = WallpaperHistory.Peek();
        FileUtils.CopyFileToDir(path, dest);
        return true;
    }
}