using System.Diagnostics;
using System.Timers;
using System.Windows;
using Irvuewin.Helpers.Utils;
using Irvuewin.Models;
using Irvuewin.Models.Unsplash;
using Irvuewin.ViewModels;
using Timer = System.Timers.Timer;

namespace Irvuewin.Helpers;

///<summary>
///Author: wangy325
///Date: 2025-06-06 10:04:10
///Desc: Wallpaper operation class
///</summary>
public static class TrayMenuHelper
{
    // Wallpaper history(file path) stack
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


    // Reset sequence when channel refreshed
    public static void ResetChannelSequence(string channelId)
    {
        CachedWallpaperSequence[channelId] = 1;
    }

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

    public static async Task ChangeCurrentWallpaper()
    {
        var channelsViewModel = await ChannelsViewModel.GetInstanceAsync();
        var channel = channelsViewModel.SelectedChannel;
        var randomWallpaper = Properties.Settings.Default.RandomWallpaper;
        if (randomWallpaper)
        {
            await SetWallPaper(channel, random: true);
        }
        else
        {
            // TODO 多显示器的栈缓存还不一样呢
            var sequence = CachedWallpaperSequence[channel.Id];
            await SetWallPaper(channel, sequence);
            var loadedPhotos = channelsViewModel.LoadedPhotoCount[channel.Id];
            Console.WriteLine($@"> loadedPhotos: {loadedPhotos}");
            if (++sequence > loadedPhotos)
            {
                sequence %= loadedPhotos;
            }

            _sequenceModify++;
            CachedWallpaperSequence[channel.Id] = sequence;
        }
    }


    private static async Task SetWallPaper(UnsplashChannel channel, int sequence = 0, bool random = false)
    {
        UnsplashPhoto? photo = null;
        var httpService = IHttpClient.GetUnsplashHttpService();
        if (random)
        {
            photo = await httpService.GetRandomPhotoInChannel(channel.Id);
        }
        else
        {
            var (shardIndex, shardPositionIndex) = CalShardIndex(sequence);
            var channelsViewModel = await ChannelsViewModel.GetInstanceAsync();
            PhotosCachePageIndex photosCachePageIndex = new()
            {
                ChannelId = channel.Id,
                PageIndex = shardIndex
            };
            // wallpaper file cache path
            if (await UnsplashCache.LoadPhotosShardAsync(photosCachePageIndex) is { } res)
            {
                // TODO
                // 0) 一定是从缓存中获取图片信息
                // 1) 数据完整性问题
                // 2) 数据过滤后，index可能越界的问题 -- 动态计算总图片数
                // 理论上每页都会存在PageSize个条目，除了最后一页
                Console.WriteLine($@"> sequence {sequence}. shard: {shardIndex}, position: {shardPositionIndex}");
                if (shardPositionIndex == res.Count - 1)
                {
                    // Preload photos
                    // Refresh loaded photos count
                    var nextPage = new PhotosCachePageIndex
                    {
                        ChannelId = channel.Id,
                        PageIndex = shardIndex + 1
                    };
                    if (await UnsplashCache.LoadPhotosShardAsync(nextPage) is null)
                    {
                        var query = new UnsplashQueryParams()
                        {
                            Page = nextPage.PageIndex,
                            PerPage = IAppConst.PageSize,
                            Orientation = Properties.Settings.Default.WallpaperOrientation
                        };
                        if (await httpService.GetPhotosOfChannel(channel.Id, query) is { } photos
                            && photos.Count != 0)
                        {
                            // Cache new photos
                            await UnsplashCache.CachePhotosAsync(nextPage, photos);
                            // Ugly
                            channelsViewModel.LoadedPhotoCount[channel.Id] += photos.Count;
                        }
                    }
                }

                photo = res[shardPositionIndex];
            }
        }

        if (photo is null) return;
        if (await WallpaperUtil.SetWallpaper(photo) is { } path)
        {
            // _currentScreen is null once app started
            CheckPointer();
            // Push photo file into wallpaper history stack
            WallpaperHistory.Push(path);
            CurrentWallpaper[_currentScreen!] = photo.Id;
            WallpaperStack[_currentScreen!] = WallpaperHistory;
            await GetWallpaperInfo();
        }
    }

    private static (int shardIndex, int shardPositionIndex) CalShardIndex(int sequence)
    {
        // page index, position index of page content
        int shardIndex, shardPositionIndex;
        // Index starts from 1
        if (sequence % IAppConst.PageSize == 0)
        {
            shardIndex = sequence / IAppConst.PageSize;
            shardPositionIndex = IAppConst.PageSize - 1;
        }
        else
        {
            shardIndex = sequence / IAppConst.PageSize + 1;
            shardPositionIndex = sequence % IAppConst.PageSize - 1;
        }

        return (shardIndex, shardPositionIndex);
    }

    // 检查指针在哪个屏幕
    public static void CheckPointer()
    {
        _currentScreen = DisplayInfoHelper.CheckDisplay().name;
    }

    public static void ChangeAllWallpaper()
    {
        // TODO change all wallpaper
    }


    private static async Task GetWallpaperInfo()
    {
        // TODO 多显示器
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

            trayViewModel!.AboutWallpaper.Likes = photo.Likes.ToString();
            trayViewModel.AboutWallpaper.Downloads = photo.Downloads.ToString();
            trayViewModel.AboutWallpaper.Location = photo.Location.Name;
            trayViewModel.AboutWallpaper.ProfileLink = photo.Links.Html.OriginalString;
            trayViewModel.AboutWallpaper.Author = photo.User.Name;
            trayViewModel.AboutWallpaper.AuthorProfilePageLink = photo.User.Links.Html.OriginalString;
        }
    }

    // TODO 多显示器
    public static async void PreviousWallpaper()
    {
        // Do nothing when stack is empty or stack has only current wallpaper
        // This happens when app starts up
        if (WallpaperHistory.Count <= 1) return;
        WallpaperHistory.Pop();
        var path = WallpaperHistory.Peek();
        await WallpaperUtil.SetWallpaper(null, path);
    }

    public static bool DownloadCurrentWallpaper(string dest)
    {
        if (WallpaperHistory.Count < 1) return false;
        var path = WallpaperHistory.Peek();
        FileUtils.CopyFileToDir(path, dest);
        return true;
    }

    public static Timer? WallpaperTimer;

    public static void InitWallpaperChangeScheduler()
    {
        // ms
        var interval = Properties.Settings.Default.WallpaperChangeInterval;
        if (interval <= 0) return;

        WallpaperTimer = new Timer(interval * 60 * 1000);
        WallpaperTimer.Elapsed += OnWallpaperTimerElapsed;
        WallpaperTimer.AutoReset = true;
        WallpaperTimer.Enabled = true;
        UpdateNextWallpaperChangeTriggerTime(interval);
    }

    public static void UpdateWallpaperChangeScheduler()
    {
        var trayViewModel = Application.Current.Resources["TrayViewModel"] as TrayViewModel;
        var interval = Properties.Settings.Default.WallpaperChangeInterval;
        // non zero -> 0
        if (interval <= 0)
        {
            // Release Timer
            WallpaperTimer?.Stop();
            WallpaperTimer = null;
            trayViewModel!.NextWallpaperChangeTime = DateTimeOffset.MinValue;
            return;
        }

        // 0 -> non zero
        if (WallpaperTimer == null)
        {
            InitWallpaperChangeScheduler();
            return;
        }

        WallpaperTimer.Stop();
        WallpaperTimer.Interval = interval * 60 * 1000;
        WallpaperTimer.Start();
        UpdateNextWallpaperChangeTriggerTime(interval);
    }

    private static void UpdateNextWallpaperChangeTriggerTime(ushort interval)
    {
        var trayViewModel = Application.Current.Resources["TrayViewModel"] as TrayViewModel;
        var nextTrigger = DateTimeOffset.Now.AddMinutes(interval);
        trayViewModel!.NextWallpaperChangeTime = nextTrigger;
        /*Application.Current.Dispatcher.InvokeAsync(() =>
        {
            trayViewModel!.NextWallpaperChangeTime = DateTimeOffset.Now.AddMilliseconds(interval);
        });*/
    }

    private static async void OnWallpaperTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            // ChangeAllWallpaper();
            // Console.WriteLine($@"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}: scheduled wallpaper change");
            await ChangeCurrentWallpaper();
            UpdateNextWallpaperChangeTriggerTime(Properties.Settings.Default.WallpaperChangeInterval);
        }
        catch (Exception ex)
        {
            // ignored
        }
    }
}