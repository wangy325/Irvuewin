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
///Core biz<br/>
///</summary>
/// <remarks>
/// In-memory Cached Fields:
/// <para>
/// 1. Channel wallpaper sequence
/// <code>
/// CacheManager.set(key1="sequence", key2=channelId, value=seq)
/// // batch set, datatype is tuple(tuple(key1, key2), val)
/// CacheManager.SetRange(range)
/// </code>
/// </para>
/// <para>2. Displays wallpaper history stack
/// <code>CacheManager.set(key1="wallpaper_stack", key2=displayName, val=stack)</code>
/// </para>
/// <para>3. Current wallpaper for each display
/// <code>CacheManager.set(key=displayName, value=photoId)</code>
/// </para>
/// </remarks>
public static class IrvuewinCore
{
    private static readonly ILogger Logger = Log.ForContext(typeof(IrvuewinCore));

    private static int _sequenceModify;

    /// <summary>
    /// Display mouse pointer is on
    /// </summary>
    public static Display CurrentPointerDisplay { get; private set; }

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
        var cid = Properties.Settings.Default.UserCheckedChannel;
        var randomWallpaper = Properties.Settings.Default.RandomWallpaper;

        if (randomWallpaper)
        {
            await SetUpWallPaper(cid, random: true, multiSetUp: multiSetUp);
        }
        else
        {
            // Multi displays share same sequence
            // So they can display different wallpapers without duplication
            var sequence = CacheManager.TryGet(CachedChannelSeqPrefix, cid, out int s) ? s : 1;
            await SetUpWallPaper(cid, sequence, multiSetUp: multiSetUp);
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
            var cid = Properties.Settings.Default.UserCheckedChannel;
            if (Properties.Settings.Default.RandomWallpaper)
            {
                // 2+ random wallpapers
                await SetUpWallPaper(cid, random: true, multiSetUp: true, sameOrNot: 1);
            }
            else
            {
                // 2+ sequence wallpapers
                var sequence = CacheManager.TryGet(CachedChannelSeqPrefix, cid, out int s) ? s : 1;
                await SetUpWallPaper(cid, sequence, multiSetUp: true, sameOrNot: 1);
            }
        }
    }

    /// <summary>
    /// Set up wallpaper.
    /// </summary>
    /// <param name="channelId">Checked channel id</param>
    /// <param name="sequence">Necessary if sequence wallpaper mode</param>
    /// <param name="random">Necessary if random wallpaper mode</param>
    /// <param name="multiSetUp">Necessary if set up multi-displays wallpaper, default false</param>
    /// <param name="sameOrNot">If multiDisplays share same wallpaper: 0 yes, 1 no</param>
    /// <returns>
    /// <para>False if failed. </para>
    /// </returns>
    private static async Task SetUpWallPaper(string channelId,
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
        var newSeq = 0;
        var httpService = IHttpClient.GetUnsplashHttpService();
        if (random)
        {
            photos = await httpService.GetRandomPhotoInChannel(channelId, wallpaperCount);
        }
        else
        {
            for (var i = sequence; i < wallpaperCount + sequence; i++)
            {
                var (s, p) = await GetSequenceWallpaper(channelId, i);
                if (p != null) photos.Add(p);
                newSeq = s;
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
                        CacheManager.Set(screen.DeviceName, photos[0].Id);
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
                        CacheManager.Set(item.kvp.Key, photos[item.idx].Id);
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
            if (await WallpaperUtil.SetWallpaperForSpecificMonitor(CurrentPointerDisplay, photos[0])
                is not { } path)
                return;
            // Push photo file into wallpaper history stack
            UpdateDisplayWallpaperStack(CurrentPointerDisplay.Name, path);
            CacheManager.Set(CurrentPointerDisplay.Name, photos[0].Id);
            WallpaperChangedEvent?.Invoke(null,
                new WallpaperChangedEventArgs(CurrentPointerDisplay.Name, photos[0].Id));
            // await DisplayWallpaperInfo();
        }

        // Update seq if necessary
        if (newSeq > 0)
        {
            _sequenceModify++;
            CacheManager.Set(CachedChannelSeqPrefix, channelId, newSeq);
        }
    }

    /// <summary>
    /// Update displays' wallpaper cache stack. 
    /// </summary>
    /// <param name="key">display name</param>
    /// <param name="value">wallpaper path</param>
    public static void UpdateDisplayWallpaperStack(string key, string value)
    {
        if (CacheManager.TryGet(CachedWallpaperStack, key, out Stack<string>? stack) && stack is not null)
        {
            stack.Push(value);
        }
        else
        {
            var initStack = new Stack<string>(capacity: 10);
            initStack.Push(value);
            CacheManager.Set(CachedWallpaperStack, key, initStack);
        }
    }

    /// <summary>
    /// Get wallpaper from channel by sequence.<br/>
    /// Activated when user de-select random wallpaper.
    /// </summary>
    /// <param name="channelId">unsplash channelId</param>
    /// <param name="sequence">current wallpaper sequence</param>
    /// <returns>sequence and wallpaper tuple</returns>
    private static async Task<(int, UnsplashPhoto?)> GetSequenceWallpaper(string channelId, int sequence)
    {
        Logger.Information(@"wallpaper seq {0} from channel {1}", sequence, channelId);

        if (!CacheManager.TryGet<List<UnsplashPhoto>>
                (CachedWallpapers, channelId, out var photos)
            || photos is null) return (sequence, null);
        // sequence should not bigger than photos size
        if (sequence < photos.Count) return (sequence + 1, photos[sequence - 1]);
        // One extreme condition: all photos already loaded, and sequence
        // is equal to photos count.
        // On this condition, we need to reset sequence.
        var cvm = await ChannelsViewModel.GetInstanceAsync();
        // preload next shard
        CacheManager.TryGet<int>(CachedWallpaperShard, channelId, out var shard);
        var query = new UnsplashQueryParams()
        {
            Page = ++shard,
            PerPage = PageSize,
            Orientation = Properties.Settings.Default.WallpaperOrientation
        };
        if (!await cvm.LoadPhotos(channelId, query, true))
        {
            // 1. All photos loaded 
            // 2. Other exceptions, like network error etc.
            return (1, photos[sequence - 1]);
        }

        CacheManager.Set(CachedWallpaperShard, channelId, shard);
        return (sequence + 1, photos[sequence - 1]);
    }

    /// <summary>
    /// Calculating pageNum and pageIndex from sequence of channel.
    /// </summary>
    /// <param name="sequence">channel wallpaper sequence</param>
    /// <returns>tuple of (page, pageIndex)</returns>
    /// <example>
    /// if sequence = 17, pageSize = 10, then
    /// shardIndex = 2, shardPositionIndex= 6
    /// </example>
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
            if (!CacheManager.TryGet<Stack<string>>(CachedWallpaperStack, CurrentPointerDisplay.Name, out var stack) ||
                stack is null)
                return;

            if (stack.Count <= 1) return;
            stack.Pop();
            var path = stack.Peek();
            await WallpaperUtil.SetWallpaperForSpecificMonitor(CurrentPointerDisplay, null, path);
            var pid = Path.GetFileNameWithoutExtension(path);
            CacheManager.Set(CurrentPointerDisplay.Name, pid);
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
        // var stack = WallpaperStack[CurrentPointerDisplay.Name];
        if (!CacheManager.TryGet<Stack<string>>(CachedWallpaperStack, CurrentPointerDisplay.Name, out var stack) ||
            stack is null) return false;
        if (stack.Count == 0) return false;
        var path = stack.Peek();
        FileUtils.CopyFileToDir(path, dest);
        return true;
    }


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