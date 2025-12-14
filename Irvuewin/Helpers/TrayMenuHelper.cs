using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows.Forms;
using Irvuewin.Helpers.Utils;
using Irvuewin.Models;
using Irvuewin.Models.Unsplash;
using Irvuewin.ViewModels;
using Application = System.Windows.Application;
using Timer = System.Timers.Timer;

namespace Irvuewin.Helpers;
using Serilog;

///<summary>
///Author: wangy325
///Date: 2025-06-06 10:04:10
///Desc: Wallpaper operation class
///</summary>
public static class TrayMenuHelper
{
    // Wallpaper history(file path) stack
    [Obsolete("Will delete in future, replace with WallpaperStack", true)]
    private static readonly Stack<string> WallpaperHistory = new(10);

    private static readonly ILogger Logger = Log.ForContext(typeof(TrayMenuHelper));

    private static int _sequenceModify;

    // Wallpaper sequence for each channel (key is channelId)
    // Used to locate wallpaper in sequence mode
    private static readonly Dictionary<string, int> CachedWallpaperSequence = new();

    // screen name, screen wallpaper stack
    private static readonly Dictionary<string, Stack<string>> WallpaperStack = new();

    public static Display CurrentScreen;

    // Current wallpaper of each screen (key is screen id, value is photoId)
    private static readonly Dictionary<string, string> CurrentWallpaper = new();

    // Flag of wallpaper changed for each screen, key is screen id
    public static readonly Dictionary<string, bool> WallpaperChanged = new();

    // Reset sequence when channel refreshed
    public static void ResetChannelSequence(string channelId)
    {
        CachedWallpaperSequence[channelId] = 1;
    }

    // Once new channel added, add its sequence to cache
    public static void AddNewChannelSequence(List<ChannelViewModel> channels)
    {
        var trayViewModel = Application.Current.Resources["TrayViewModel"] as TrayViewModel;
        foreach (var channel in channels)
        {
            CachedWallpaperSequence[channel.Id] = 1;
            trayViewModel!.AddedChannels.Add(channel);
        }

        _sequenceModify++;
    }

    public static void DelChannelSequence(string key)
    {
        var trayViewModel = Application.Current.Resources["TrayViewModel"] as TrayViewModel;
        CachedWallpaperSequence.Remove(key);
        _sequenceModify++;
        var filteredList = trayViewModel!.AddedChannels.Where(channel => channel.Id != key).ToList();

        trayViewModel.AddedChannels = [..filteredList];
        /*foreach (var channel in filteredList)
        {
            trayViewModel.Channels.Add(channel);
        }*/
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

        // Log.Debug($@">>> Load Cached wallpaper sequence: {CachedWallpaperSequence.Count}");
    }

    public static async void SaveCachedSequence()
    {
        if (_sequenceModify <= 0) return;
        await UnsplashCache.CacheChannelSequence(CachedWallpaperSequence);
        // Console.WriteLine($@">>> Save Cached wallpaper sequence: {CachedWallpaperSequence}");
        // reset
        _sequenceModify = 0;
    }

    // 检查指针在哪个屏幕
    public static void CheckPointer()
    {
        CurrentScreen = DisplayInfoHelper.CheckCursorPosition();
    }

    /// <summary>
    /// Change current display's wallpaper
    /// </summary>
    /// <param name="multiSetUp">false by default. For resuing.</param>
    public static async Task ChangeCurrentWallpaper(bool multiSetUp = false)
    {
        var cvm = await ChannelsViewModel.GetInstanceAsync();
        var channel = cvm.Channels.First(c => c.IsChecked);
        var randomWallpaper = Properties.Settings.Default.RandomWallpaper;

        if (randomWallpaper)
        {
            await SetUpWallPaper(channel, random: true, multiSetUp: multiSetUp);
        }
        else
        {
            // 多显示器的栈缓存还不一样呢
            // does not matter, multi displays share same sequence
            // so they can display different wallpapers without duplication
            var sequence = CachedWallpaperSequence[channel.Id];
            var loadedPhotos = cvm.LoadedPhotoCount[channel.Id];
            // Do nothing when collections can not load photo(s) through api
            if (loadedPhotos == 0) return;
            await SetUpWallPaper(channel, sequence, multiSetUp: multiSetUp);
            Logger.Information(@"> loadedPhotos: {LoadedPhotos}", loadedPhotos);
            if (++sequence > loadedPhotos)
            {
                sequence %= loadedPhotos;
            }

            _sequenceModify++;
            CachedWallpaperSequence[channel.Id] = sequence;
        }
    }


    /// <summary>
    /// Set up wallpaper.
    /// </summary>
    /// <param name="channel">wallpaper channel</param>
    /// <param name="sequence">necessary if sequence wallpaper mode</param>
    /// <param name="random">necessary if random wallpaper mode</param>
    /// <param name="multiSetUp">necessary if set up multi-displays wallpaper, default false</param>
    /// <param name="sameOrNot">if multiDisplay share same wallpaper: 0 yes, 1 no</param>
    private static async Task<bool> SetUpWallPaper(
        UnsplashChannel channel,
        int sequence = 0,
        bool random = false,
        bool multiSetUp = false,
        byte sameOrNot = 0)
    {
        var mc = Screen.AllScreens.Length;
        var pc = 1;
        if (sameOrNot == 1 && mc > 1)
        {
            // 2+
            pc = mc;
        }

        List<UnsplashPhoto>? photos = [];
        var httpService = IHttpClient.GetUnsplashHttpService();
        if (random)
        {
            photos = await httpService.GetRandomPhotoInChannel(channel.Id, pc);
        }
        else
        {
            for (var i = sequence; i < pc + sequence; i++)
            {
                var p = await GetSequenceWallpaper(channel, i);
                if (p != null)
                {
                    photos.Add(p);
                }
            }
        }

        if (photos == null || photos.Count == 0)
        {
            Logger.Debug($@"Get photo from channel error.");
            return false;
        }


        // Already get wallpaper(s)
        if (multiSetUp)
        {
            var setUpRes = await WallpaperUtil.SetWallpaper(photos);
            switch (setUpRes)
            {
                case null:
                    return false;
                case { IsUnified: true }:
                {
                    // single wallpaper
                    // update all display(s) history stack
                    foreach (var screen in Screen.AllScreens)
                    {
                        if (WallpaperStack.TryGetValue(screen.DeviceName, out var stack))
                        {
                            stack.Push(setUpRes.UnifiedWallpaperPath!);
                        }
                        else
                        {
                            var initStack = new Stack<string>(capacity: 10);
                            initStack.Push(setUpRes.UnifiedWallpaperPath!);
                            WallpaperStack[screen.DeviceName] = initStack;
                        }

                        CurrentWallpaper[screen.DeviceName] = photos[0].Id;
                        WallpaperChanged[screen.DeviceName] = true;
                    }

                    break;
                }
                case { IsUnified: false }:
                {
                    // multi wallpapers
                    var res = setUpRes.PerDisplayWallpapers!;
                    foreach (var item in res.Select((kvp, idx) => new { kvp, idx }))
                    {
                        if (WallpaperStack.TryGetValue(item.kvp.Key, out var stack))
                        {
                            stack.Push(item.kvp.Value);
                        }
                        else
                        {
                            var initStack = new Stack<string>(capacity: 10);
                            initStack.Push(item.kvp.Value);
                            WallpaperStack[item.kvp.Key] = initStack;
                        }

                        CurrentWallpaper[item.kvp.Key] = photos[item.idx].Id;
                        WallpaperChanged[item.kvp.Key] = true;
                        Logger.Information(@"Set wallpaper for {Key}: {Value}", item.kvp.Key, item.kvp.Value);
                    }

                    break;
                }
            }
        }
        else
        {
            // Setup single display's wallpaper
            if (await WallpaperUtil.SetWallpaperForSpecificMonitor(CurrentScreen, photos[0]) is not
                { } path) return false;
            // Push photo file into wallpaper history stack
            if (WallpaperStack.TryGetValue(CurrentScreen.Name, out var s))
            {
                s.Push(path);
            }
            else
            {
                var initStack = new Stack<string>(capacity: 10);
                initStack.Push(path);
                WallpaperStack[CurrentScreen.Name] = initStack;
            }

            CurrentWallpaper[CurrentScreen.Name] = photos[0].Id;
            WallpaperChanged[CurrentScreen.Name] = true;
            // await DisplayWallpaperInfo();
        }

        return true;
    }

    /// <summary>
    /// Get wallpaper from channel
    /// </summary>
    /// <param name="channel">unsplash channel</param>
    /// <param name="sequence">current wallpaper sequence</param>
    /// <returns>wallpaper, nullable </returns>
    private static async Task<UnsplashPhoto?> GetSequenceWallpaper(UnsplashChannel channel, int sequence)
    {
        Logger.Information(@"Get wallpaper sequence: {Sequence}", sequence);
        var (shardIndex, shardPositionIndex) = CalShardIndex(sequence);
        var channelsViewModel = await ChannelsViewModel.GetInstanceAsync();
        PhotosCachePageIndex photosCachePageIndex = new()
        {
            ChannelId = channel.Id,
            PageIndex = shardIndex
        };
        // wallpaper file cache path
        if (await UnsplashCache.LoadPhotosShardAsync(photosCachePageIndex) is not { } res) return null;
        if (!res.Any()) return null;
        // 0) 一定是从缓存record中获取图片信息
        // 1) 数据完整性问题
        // 2) 数据过滤后，index可能越界的问题 -- 动态计算总图片数
        // 理论上每页都会存在PageSize个条目，除了最后一页
        Logger.Information(@"> sequence {Sequence}. shard: {ShardIndex}, position: {ShardPositionIndex}", sequence, shardIndex, shardPositionIndex);
        if (shardPositionIndex == res.Count - 1)
        {
            await PreloadChannelNextPage(channel, shardIndex, channelsViewModel);
        }

        return res[shardPositionIndex];
    }

    private static async Task PreloadChannelNextPage(
        UnsplashChannel channel,
        int shardIndex,
        ChannelsViewModel channelsViewModel)
    {
        // Next page
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
            if (await IHttpClient.GetUnsplashHttpService().GetPhotosOfChannel(channel.Id, query) is { } photos
                && photos.Count != 0)
            {
                // Cache new photos
                await UnsplashCache.CachePhotosAsync(nextPage, photos);
                // Ugly
                channelsViewModel.LoadedPhotoCount[channel.Id] += photos.Count;
            }
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


    public static async Task ChangeAllWallpaper()
    {
        var cvm = await ChannelsViewModel.GetInstanceAsync();
        var channel = cvm.Channels.First(c => c.IsChecked);
        var randomWallpaper = Properties.Settings.Default.RandomWallpaper;
        var sameOrNot = Properties.Settings.Default.MultiDisplay;

        if (sameOrNot == 0)
        {
            await ChangeCurrentWallpaper(true);
        }
        else
        {
            if (randomWallpaper)
            {
                // 2+ random wallpapers
                await SetUpWallPaper(channel, random: true, multiSetUp: true, sameOrNot: 1);
            }
            else
            {
                // 2+ sequence wallpapers
                var sequence = CachedWallpaperSequence[channel.Id];
                var loadedPhotos = cvm.LoadedPhotoCount[channel.Id];
                // Do nothing when collections can not load photo(s) through api
                if (loadedPhotos == 0) return;
                await SetUpWallPaper(channel, sequence, multiSetUp: true, sameOrNot: 1);
                Logger.Information(@"> loadedPhotos: {LoadedPhotos}", loadedPhotos);
                var mc = Screen.AllScreens.Length;
                sequence += mc;
                if (sequence > loadedPhotos)
                {
                    sequence %= loadedPhotos;
                }

                _sequenceModify++;
                CachedWallpaperSequence[channel.Id] = sequence;
            }
        }
    }


    /// <summary>
    /// Display wallpaper info on tray menu
    /// </summary>
    public static async Task DisplayWallpaperInfo()
    {
        Logger.Information(@"wallpaper info update");
        if (!CurrentWallpaper.TryGetValue(CurrentScreen.Name, out var photoId)) return;

        // update all screen?
        /*foreach (var kvp in CurrentWallpaper)
        {

        }*/

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

    public static async void PreviousWallpaper()
    {
        // Do nothing when stack is empty or stack has only 1 wallpaper
        // This happens when app starts up
        var stack = WallpaperStack[CurrentScreen.Name];

        if (stack.Count <= 1) return;
        stack.Pop();
        var path = stack.Peek();
        await WallpaperUtil.SetWallpaperForSpecificMonitor(CurrentScreen, null, path);
        var pid = Path.GetFileNameWithoutExtension(path);
        CurrentWallpaper[CurrentScreen.Name] = pid;
        WallpaperChanged[CurrentScreen.Name] = true;
        // await DisplayWallpaperInfo();
    }

    public static bool DownloadCurrentWallpaper(string dest)
    {
        var stack = WallpaperStack[CurrentScreen.Name];

        if (stack.Count == 0) return false;
        var path = stack.Peek();
        FileUtils.CopyFileToDir(path, dest);
        return true;
    }

    private static Timer? _wallpaperTimer;

    public static void InitWallpaperChangeScheduler()
    {
        // ms
        var interval = Properties.Settings.Default.WallpaperChangeInterval;
        if (interval <= 0) return;

        _wallpaperTimer = new Timer(interval * 60 * 1000);
        _wallpaperTimer.Elapsed += OnWallpaperTimerElapsed;
        _wallpaperTimer.AutoReset = true;
        _wallpaperTimer.Enabled = true;
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
            _wallpaperTimer?.Stop();
            _wallpaperTimer = null;
            trayViewModel!.NextWallpaperChangeTime = DateTimeOffset.MinValue;
            return;
        }

        // 0 -> non zero
        if (_wallpaperTimer == null)
        {
            InitWallpaperChangeScheduler();
            return;
        }

        _wallpaperTimer.Stop();
        _wallpaperTimer.Interval = interval * 60 * 1000;
        _wallpaperTimer.Start();
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
            await ChangeAllWallpaper();
            UpdateNextWallpaperChangeTriggerTime(Properties.Settings.Default.WallpaperChangeInterval);
        }
        catch (Exception ex)
        {
            // ignored
        }
    }
}