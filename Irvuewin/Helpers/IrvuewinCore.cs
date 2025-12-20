using System.IO;
using System.Timers;
using System.Windows.Forms;
using Irvuewin.Helpers.Utils;
using Irvuewin.Models.Unsplash;
using Irvuewin.ViewModels;
using static Irvuewin.Helpers.IAppConst;
using Application = System.Windows.Application;
using Timer = System.Timers.Timer;

namespace Irvuewin.Helpers;

using Serilog;

///<summary>
///Author: wangy325
///Date: 2025-06-06 10:04:10
///Desc: Core biz
///</summary>
public static class IrvuewinCore
{
    private static readonly ILogger Logger = Log.ForContext(typeof(IrvuewinCore));

    private static int _sequenceModify;

    /// <summary>
    /// Display mouse pointer is on
    /// </summary>
    public static Display CurrentPointerDisplay { get; private set; }

    /// <summary>
    /// Wallpaper record for each display. <br/>
    /// Key is display/screen name, Value is a wallpaper stack with capacity 10.
    /// </summary>
    private static Dictionary<string, Stack<string>> WallpaperStack { get; } = new();

    /// <summary>
    /// Current wallpaper of each display (key is screen name, value is photoId).
    /// </summary>
    public static Dictionary<string, string> CurrentWallpapers { get; } = new();

    public class WallpaperChangedEventArgs(string displayName, string photoId) : EventArgs
    {
        public string DisplayName { get; } = displayName;
        public string PhotoId { get; } = photoId;
    }

    public static event EventHandler<WallpaperChangedEventArgs>? WallpaperChangedEvent;

    public static void BroadcastWallpaperChanged(string displayName, string photoId)
    {
        WallpaperChangedEvent?.Invoke(null, new WallpaperChangedEventArgs(displayName, photoId));
    }


    /// ################################## Methods #################################### ///
    /// <summary>
    /// Reset sequence when channel refreshed.
    /// </summary>
    /// <param name="channelId"></param>
    public static void ResetChannelSequence(string channelId)
    {
        CacheManager.Set(CachedChannelSeqPrefix, channelId, 1);
    }

    /// <summary>
    /// Once new channel(s) is added, add its sequence to cache.
    /// </summary>
    /// <param name="channels"/>
    public static void AddNewChannelSequence(List<ChannelViewModel> channels)
    {
        var trayViewModel = Application.Current.Resources["TrayViewModel"] as TrayViewModel;
        foreach (var channel in channels)
        {
            CacheManager.Set(CachedChannelSeqPrefix, channel.Id, 1);
            trayViewModel!.AddedChannels.Add(channel);
        }

        _sequenceModify++;
    }

    /// <summary>
    /// Delete cached channel sequence by channel ID.
    /// </summary>
    /// <param name="key">Channel ID</param>
    public static void DelChannelSequence(string key)
    {
        var trayViewModel = Application.Current.Resources["TrayViewModel"] as TrayViewModel;
        CacheManager.Set(CachedChannelSeqPrefix, key, 1, TimeSpan.Zero);
        _sequenceModify++;
        var filteredList = trayViewModel!.AddedChannels.Where(channel => channel.Id != key).ToList();
        trayViewModel.AddedChannels = [..filteredList];
    }

    /// <summary>
    /// Load/Init All channels' cached sequences.
    /// </summary>
    public static async Task LoadCachedSequence()
    {
        var sequence = await FileCacheManager.LoadChannelSequence();
        if (sequence is not null && sequence.Count != 0)
        {
            var range
                = sequence.Select(kvp => ((CachedChannelSeqPrefix, kvp.Key), kvp.Value));
            CacheManager.SetRange(range);
        }
        else
        {
            var channels = Properties.Settings.Default.UserUnsplashChannels.Split(",");
            var range
                = channels.Select(c => ((CachedChannelSeqPrefix, c), 1));
            // Init in-memory channel sequence cacheZ
            CacheManager.SetRange(range);
        }

        Logger.Debug(@"Init channels' cached sequence.");
    }

    /// <summary>
    /// Persisting all channels' cached sequence.
    /// </summary>
    public static void SaveCachedSequence()
    {
        if (_sequenceModify <= 0) return;

        var dic = new Dictionary<string, int>();
        var channels = Properties.Settings.Default.UserUnsplashChannels.Split(",");
        foreach (var cid in channels)
        {
            if (CacheManager.TryGet(CachedChannelSeqPrefix, cid, out int seq))
            {
                dic[cid] = seq;
            }
        }

        _ = Task.Run(() => FileCacheManager.CacheChannelSequence(dic));
        // reset
        _sequenceModify = 0;
        Logger.Debug(@"Save cached wallpaper sequence.");
    }

    /// <summary>
    /// Check chich display current mouse pointer is on. 
    /// </summary>
    public static void CheckPointer()
    {
        CurrentPointerDisplay = DisplayInfoHelper.CheckCursorPosition();
    }

    /// <summary>
    /// Change current display's wallpaper from tray command.
    /// </summary>
    /// <param name="multiSetUp">False by default.
    /// True means method will set up wallpaper for multi displays.</param>
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
            // Multi displays share same sequence
            // So they can display different wallpapers without duplication
            var sequence = CacheManager.TryGet(CachedChannelSeqPrefix, channel.Id, out int s) ? s : 1;
            var loadedPhotos = cvm.LoadedPhotoCount[channel.Id];
            // Do nothing when collections can not load photo(s) through api
            if (loadedPhotos == 0) return;
            await SetUpWallPaper(channel, sequence, multiSetUp: multiSetUp);
            // Logger.Information(@"> loadedPhotos: {0}", loadedPhotos);
            if (++sequence > loadedPhotos)
            {
                sequence %= loadedPhotos;
            }

            _sequenceModify++;
            CacheManager.Set(CachedChannelSeqPrefix, channel.Id, sequence);
        }
    }

    /// <summary>
    /// Setup all displays wallpaper from tray command.
    /// </summary>
    public static async Task ChangeAllWallpaper()
    {
        var sameOrNot = Properties.Settings.Default.MultiDisplay;

        if (sameOrNot == 0)
        {
            await ChangeCurrentWallpaper(true);
        }
        else
        {
            var cvm = await ChannelsViewModel.GetInstanceAsync();
            var channel = cvm.Channels.First(c => c.IsChecked);
            if (Properties.Settings.Default.RandomWallpaper)
            {
                // 2+ random wallpapers
                await SetUpWallPaper(channel, random: true, multiSetUp: true, sameOrNot: 1);
            }
            else
            {
                // 2+ sequence wallpapers
                var sequence = CacheManager.TryGet(CachedChannelSeqPrefix, channel.Id, out int s) ? s : 1;
                var loadedPhotos = cvm.LoadedPhotoCount[channel.Id];
                // Do nothing when collections can not load photo(s) through api
                if (loadedPhotos == 0) return;
                await SetUpWallPaper(channel, sequence, multiSetUp: true, sameOrNot: 1);
                // Logger.Information(@"> loadedPhotos: {LoadedPhotos}", loadedPhotos);
                var mc = Screen.AllScreens.Length;
                sequence += mc;
                if (sequence > loadedPhotos)
                {
                    sequence %= loadedPhotos;
                }

                _sequenceModify++;
                CacheManager.Set(CachedChannelSeqPrefix, channel.Id, sequence);
            }
        }
    }

    /// <summary>
    /// Set up wallpaper.
    /// </summary>
    /// <param name="channel">Checked channel</param>
    /// <param name="sequence">Necessary if sequence wallpaper mode</param>
    /// <param name="random">Necessary if random wallpaper mode</param>
    /// <param name="multiSetUp">Necessary if set up multi-displays wallpaper, default false</param>
    /// <param name="sameOrNot">If multiDisplays share same wallpaper: 0 yes, 1 no</param>
    /// <returns>
    /// <para>False if failed. </para>
    /// </returns>
    private static async Task SetUpWallPaper(UnsplashChannel channel,
        int sequence = 0,
        bool random = false,
        bool multiSetUp = false,
        byte sameOrNot = 0)
    {
        var displayCount = Screen.AllScreens.Length;
        var wallpaperCount = 1;
        if (sameOrNot == 1 && displayCount > 1)
        {
            // 2+
            wallpaperCount = displayCount;
        }

        List<UnsplashPhoto>? photos = [];
        var httpService = IHttpClient.GetUnsplashHttpService();
        if (random)
        {
            photos = await httpService.GetRandomPhotoInChannel(channel.Id, wallpaperCount);
        }
        else
        {
            for (var i = sequence; i < displayCount + sequence; i++)
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
            Logger.Error("Get photo(s) from channel error.");
            return;
        }

        // Already get wallpaper(s)
        if (multiSetUp)
        {
            var setUpRes = await WallpaperUtil.SetWallpaper(photos);
            switch (setUpRes)
            {
                case null:
                    return;
                case { IsUnified: true }:
                {
                    // single wallpaper
                    // update all display(s) history stack
                    foreach (var screen in Screen.AllScreens)
                    {
                        UpdateDisplayWallpaperStack(screen.DeviceName, setUpRes.UnifiedWallpaperPath!);
                        CurrentWallpapers[screen.DeviceName] = photos[0].Id;
                        WallpaperChangedEvent?.Invoke(null,
                            new WallpaperChangedEventArgs(screen.DeviceName, photos[0].Id));
                    }

                    break;
                }
                case { IsUnified: false }:
                {
                    // multi wallpapers
                    var res = setUpRes.PerDisplayWallpapers!;
                    foreach (var item in res.Select((kvp, idx) => new { kvp, idx }))
                    {
                        UpdateDisplayWallpaperStack(item.kvp.Key, item.kvp.Value);
                        CurrentWallpapers[item.kvp.Key] = photos[item.idx].Id;
                        WallpaperChangedEvent?.Invoke(null,
                            new WallpaperChangedEventArgs(item.kvp.Key, photos[item.idx].Id));
                        // Logger.Information(@"Set wallpaper for {Key}: {Value}", item.kvp.Key, item.kvp.Value);
                    }

                    break;
                }
            }
        }
        else
        {
            // Setup single display's wallpaper
            if (await WallpaperUtil.SetWallpaperForSpecificMonitor(CurrentPointerDisplay, photos[0]) is not
                { } path)
                return;
            // Push photo file into wallpaper history stack
            UpdateDisplayWallpaperStack(CurrentPointerDisplay.Name, path);
            CurrentWallpapers[CurrentPointerDisplay.Name] = photos[0].Id;
            WallpaperChangedEvent?.Invoke(null,
                new WallpaperChangedEventArgs(CurrentPointerDisplay.Name, photos[0].Id));
            // await DisplayWallpaperInfo();
        }
    }

    /// <summary>
    /// Update displays' wallpaper cache stack. 
    /// </summary>
    /// <param name="key">display name</param>
    /// <param name="value">wallpaper path</param>
    public static void UpdateDisplayWallpaperStack(string key, string value)
    {
        if (WallpaperStack.TryGetValue(key, out var stack))
        {
            stack.Push(value);
        }
        else
        {
            var initStack = new Stack<string>(capacity: 10);
            initStack.Push(value);
            WallpaperStack[key] = initStack;
        }
    }

    /// <summary>
    /// Get wallpaper from channel sequence
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
        if (await FileCacheManager.LoadPhotosShardAsync(photosCachePageIndex) is not { } res) return null;
        if (res.Count == 0) return null;
        // 0) 一定是从缓存record中获取图片信息
        // 1) 数据完整性问题
        // 2) 数据过滤后，index可能越界的问题 -- 动态计算总图片数
        // 理论上每页都会存在PageSize个条目，除了最后一页
        Logger.Information(@"> sequence {Sequence}. shard: {ShardIndex}, position: {ShardPositionIndex}", sequence,
            shardIndex, shardPositionIndex);
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
        if (await FileCacheManager.LoadPhotosShardAsync(nextPage) is null)
        {
            var query = new UnsplashQueryParams()
            {
                Page = nextPage.PageIndex,
                PerPage = PageSize,
                Orientation = Properties.Settings.Default.WallpaperOrientation
            };
            if (await IHttpClient.GetUnsplashHttpService().GetPhotosOfChannel(channel.Id, query) is { } photos
                && photos.Count != 0)
            {
                // Cache new photos
                await FileCacheManager.CachePhotosAsync(nextPage, photos);
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
        if (sequence % PageSize == 0)
        {
            shardIndex = sequence / PageSize;
            shardPositionIndex = PageSize - 1;
        }
        else
        {
            shardIndex = sequence / PageSize + 1;
            shardPositionIndex = sequence % PageSize - 1;
        }

        return (shardIndex, shardPositionIndex);
    }


    /// <summary>
    /// Update wallpaper info on tray menu
    /// </summary>
    [Obsolete("Logic moved to TrayViewModel via WallpaperChangedEvent", false)]
    public static Task UpdateDisplayWallpaperInfo()
    {
        // Logic moved to TrayViewModel
        return Task.CompletedTask;
    }

    /// <summary>
    /// Switch back to last wallpaper.
    /// </summary>
    public static async void PreviousWallpaper()
    {
        try
        {
            // Do nothing when stack is empty or stack has only 1 wallpaper
            // This happens when app starts up
            var stack = WallpaperStack[CurrentPointerDisplay.Name];

            if (stack.Count <= 1) return;
            stack.Pop();
            var path = stack.Peek();
            await WallpaperUtil.SetWallpaperForSpecificMonitor(CurrentPointerDisplay, null, path);
            var pid = Path.GetFileNameWithoutExtension(path);
            CurrentWallpapers[CurrentPointerDisplay.Name] = pid;
            WallpaperChangedEvent?.Invoke(null, new WallpaperChangedEventArgs(CurrentPointerDisplay.Name, pid));
            // await DisplayWallpaperInfo();
        }
        catch (Exception e)
        {
            // throw;
            Logger.Error("Setting wallpaper error: {0}", e.Message);
        }
    }

    /// <summary>
    /// Download current wallpaper to specify folder.
    /// </summary>
    /// <param name="dest">dest folder to save wallpaper</param>
    /// <returns>true if success</returns>
    public static bool DownloadCurrentWallpaper(string dest)
    {
        var stack = WallpaperStack[CurrentPointerDisplay.Name];

        if (stack.Count == 0) return false;
        var path = stack.Peek();
        FileUtils.CopyFileToDir(path, dest);
        return true;
    }

    /// ##################### Wallpaper change Timer ###################### ///
    /// <summary>
    /// Wallpaper auto change scheduler.
    /// </summary>
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
            Logger.Error("Scheduling setting wallpaper error: {0}", ex.Message);
        }
    }
}