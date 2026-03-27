using System.IO;
using System.Timers;
using System.Windows.Forms;
using Irvuewin.Helpers.DB;
using Irvuewin.Helpers.Events;
using Irvuewin.Helpers.HTTP;
using Irvuewin.Helpers.Utils;
using Irvuewin.Models.Unsplash;
using Irvuewin.ViewModels;
using Microsoft.Win32;
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

    /// <summary>
    /// Display mouse pointer is on
    /// </summary>
    public static Display CurrentPointerDisplay { get; private set; }


    /// <summary>
    /// Once new channel(s) is added, add to tray menu panel.
    /// </summary>
    /// <param name="channels"/>
    public static void AddNewChannel(List<ChannelViewModel> channels)
    {
        var trayViewModel = Application.Current.Resources["TrayViewModel"] as TrayViewModel;
        foreach (var channel in channels)
        {
            trayViewModel!.AddedChannels.Add(channel);
        }
    }

    /// <summary>
    /// Delete cached channel sequence by channel ID.
    /// </summary>
    /// <param name="key">Channel ID</param>
    public static void DelChannel(string key)
    {
        var trayViewModel = Application.Current.Resources["TrayViewModel"] as TrayViewModel;
        var filteredList = trayViewModel!.AddedChannels.Where(channel => channel.Id != key).ToList();
        trayViewModel.AddedChannels = [..filteredList];
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
    public static async void ChangeCurrentWallpaper(bool multiSetUp = false)
    {
        try
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
                var channel = DataBaseService.GetChannel(cid)!;
                await SetUpWallPaper(cid, channel.Sequence, multiSetUp: multiSetUp);
            }
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// Setup all displays wallpaper from tray command.
    /// </summary>
    public static async void ChangeAllWallpaper()
    {
        try
        {
            var sameOrNot = Properties.Settings.Default.MultiDisplay;

            if (sameOrNot == 0)
            {
                ChangeCurrentWallpaper(true);
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
                    var channel = DataBaseService.GetChannel(cid)!;
                    await SetUpWallPaper(cid, channel.Sequence, multiSetUp: true, sameOrNot: 1);
                }
            }
        }
        catch
        {
            // ignore
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
    private static async Task SetUpWallPaper(
        string channelId,
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
            if (channelId == LikesChannelId)
            {
                photos = DataBaseService.GetRandomLikedPhotos(wallpaperCount);
            }
            else
            {
                photos = await httpService.GetRandomPhotoInChannel(channelId, wallpaperCount);
            }
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
                        FastCacheManager.Set(screen.DeviceName, photos[0].Id);
                        EventBus.PublishWallpaperChanged(screen.DeviceName, photos[0].Id);
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
                        FastCacheManager.Set(item.kvp.Key, photos[item.idx].Id);
                        EventBus.PublishWallpaperChanged(item.kvp.Key, photos[item.idx].Id);
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
            FastCacheManager.Set(CurrentPointerDisplay.Name, photos[0].Id);
            EventBus.PublishWallpaperChanged(CurrentPointerDisplay.Name, photos[0].Id);
            // await DisplayWallpaperInfo();
        }

        // Update seq if necessary
        if (newSeq > 0)
        {
            var channel = DataBaseService.GetChannel(channelId)!;
            channel.Sequence = newSeq;
            await DataBaseService.UpdateChannel(channel);
        }
    }

    /// <summary>
    /// Update displays' wallpaper cache stack. 
    /// </summary>
    /// <param name="key">display name</param>
    /// <param name="value">wallpaper path</param>
    public static void UpdateDisplayWallpaperStack(string key, string value)
    {
        if (FastCacheManager.TryGet(CachedWallpaperStack, key, out Stack<string>? stack) && stack is not null)
        {
            stack.Push(value);
        }
        else
        {
            var initStack = new Stack<string>(capacity: 10);
            initStack.Push(value);
            FastCacheManager.Set(CachedWallpaperStack, key, initStack);
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

        var channel = DataBaseService.GetChannel(channelId)!;
        var totalLoaded = DataBaseService.LoadedPhotosCountExcluded(channelId);
        
        if (sequence <= totalLoaded)
        {
            var photo = DataBaseService.GetPhotoBySequence(channelId, sequence)!;
            return (sequence + 1, photo);
        }

        // Likes channel is local only, no fetching from web
        if (channelId == LikesChannelId)
        {
            sequence = 1;
            var photo = DataBaseService.GetPhotoBySequence(channelId, sequence);
            return (sequence + 1, photo);
        }

        // One extreme condition: all photos already loaded, and sequence
        // is equal to (or exceeds) photos count.
        // On this condition, we need to reset sequence completely to maintain the loop without getting stuck.
        if (channel.AllPhotosLoaded)
        {
            sequence = 1;
            var loopedPhoto = DataBaseService.GetPhotoBySequence(channelId, sequence);
            return (sequence + 1, loopedPhoto);
        }
        else
        {
            // Instead of firing an asynchronous UI event that returns early, we explicitly ask the manager
            // to fetch another page from Unsplash and block until it completes.
            var success = await WallpaperPoolManager.Instance.FetchWallpapersAsync(channelId);

            if (success)
            {
                // Re-evaluate if we now have enough valid photos to satisfy the sequence.
                totalLoaded = DataBaseService.LoadedPhotosCountExcluded(channelId);
                if (sequence <= totalLoaded)
                {
                    var fetchedPhoto = DataBaseService.GetPhotoBySequence(channelId, sequence);
                    return (sequence + 1, fetchedPhoto);
                }
            }

            // 1) The network request failed or is already fetching.
            // 2) Or the API returned photos but the filters rejected ALL of them
            // In either case, we fall back to sequence 1 temporarily so the auto-change scheduler successfully changes the wallpaper
            // instead of aborting and leaving the user with a frozen sequence.
            sequence = 1;
            var fallbackPhoto = DataBaseService.GetPhotoBySequence(channelId, sequence);
            return (sequence + 1, fallbackPhoto);
        }
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
            if (!FastCacheManager.TryGet<Stack<string>>(CachedWallpaperStack, CurrentPointerDisplay.Name,
                    out var stack) ||
                stack is null)
                return;

            if (stack.Count <= 1) return;
            stack.Pop();
            var path = stack.Peek();
            await WallpaperUtil.SetWallpaperForSpecificMonitor(CurrentPointerDisplay, null, path);
            var pid = Path.GetFileNameWithoutExtension(path);
            FastCacheManager.Set(CurrentPointerDisplay.Name, pid);
            EventBus.PublishWallpaperChanged(CurrentPointerDisplay.Name, pid);
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
        if (!FastCacheManager.TryGet<Stack<string>>(CachedWallpaperStack, CurrentPointerDisplay.Name, out var stack) ||
            stack is null) return false;
        if (stack.Count == 0) return false;
        var path = stack.Peek();
        FileUtils.CopyFileToDir(path, dest);
        return true;
    }

    /// <summary>
    /// Once user change wallpaper orientation in settings window.<br/>
    /// All channels cached (in memory and local disk) photos should be reloaded.
    /// </summary>
    /// <returns></returns>
    public static async Task RefreshAllCachedWallpapers()
    {
        var cvm = await ChannelsViewModel.GetInstanceAsync();
        await cvm.RefreshPhotos();
    }

    public static void UpdateAndSaveProperties((string, object) pair)
    {
        Properties.Settings.Default[pair.Item1] = pair.Item2;
        Properties.Settings.Default.Save();
        Logger.Information(@"val of {0}: {1}", pair.Item1, Properties.Settings.Default[pair.Item1]);
    }


    /// <summary>
    /// Wallpaper auto change scheduler.
    /// </summary>
    private static Timer? _wallpaperTimer;

    /// <summary>
    /// Init wallpaper change timer when app startup
    /// </summary>
    public static void InitWallpaperChangeScheduler()
    {
        var interval = Properties.Settings.Default.WallpaperChangeInterval;
        if (interval <= 0) return;

        var fullIntervalMs = interval * 60 * 1000d;
        var startIntervalMs = fullIntervalMs;

        // Check persisted next trigger time (survives sleep / crash / restart)
        var persisted = Properties.Settings.Default.NextWallpaperChangeTime;
        if (!string.IsNullOrEmpty(persisted) &&
            DateTimeOffset.TryParse(persisted, out var nextTrigger))
        {
            var remaining = (nextTrigger - DateTimeOffset.Now).TotalMilliseconds;
            if (remaining <= 0)
            {
                // Missed trigger (sleep / crash) → change immediately, then full interval
                Logger.Information("Missed scheduled wallpaper change (was due {0}), triggering now.", nextTrigger);
                ChangeAllWallpaper();
            }
            else
            {
                // Still time left → use the remaining time for the first tick
                startIntervalMs = remaining;
            }
        }

        _wallpaperTimer = new Timer(startIntervalMs);
        _wallpaperTimer.Elapsed += OnWallpaperTimerElapsed;
        _wallpaperTimer.AutoReset = false; // first tick may be a partial interval
        _wallpaperTimer.Enabled = true;
        UpdateNextWallpaperChangeTriggerTime(startIntervalMs);

        // Subscribe to power events for sleep/resume handling
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    /// <summary>
    /// Setup/Update wallpaper change timer when user selected manually on tray menu.
    /// </summary>
    public static void UpdateWallpaperChangeScheduler()
    {
        var interval = Properties.Settings.Default.WallpaperChangeInterval;
        // non zero -> 0
        if (interval <= 0)
        {
            // Release Timer & power event
            _wallpaperTimer?.Stop();
            _wallpaperTimer = null;
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var trayViewModel = Application.Current.Resources["TrayViewModel"] as TrayViewModel;
                trayViewModel!.NextWallpaperChangeTime = DateTimeOffset.MinValue;
            });
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
        _wallpaperTimer.AutoReset = true;
        _wallpaperTimer.Start();
        UpdateNextWallpaperChangeTriggerTime(interval);
    }

    /// <summary>
    /// recalculate next timer trigger time
    /// </summary>
    /// <param name="intervalMinutes"></param>
    private static void UpdateNextWallpaperChangeTriggerTime(ushort intervalMinutes)
    {
        UpdateNextWallpaperChangeTriggerTime(intervalMinutes * 60 * 1000d);
    }

    private static void UpdateNextWallpaperChangeTriggerTime(double intervalMs)
    {
        var nextTrigger = DateTimeOffset.Now.AddMilliseconds(intervalMs);
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var trayViewModel = Application.Current.Resources["TrayViewModel"] as TrayViewModel;
            trayViewModel!.NextWallpaperChangeTime = nextTrigger;
        });
    }

    /// <summary>
    /// Handle system sleep / resume to keep the wallpaper schedule accurate.
    /// </summary>
    private static void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode != PowerModes.Resume) return;
        if (_wallpaperTimer == null) return;

        var interval = Properties.Settings.Default.WallpaperChangeInterval;
        if (interval <= 0) return;

        var persisted = Properties.Settings.Default.NextWallpaperChangeTime;
        if (string.IsNullOrEmpty(persisted) ||
            !DateTimeOffset.TryParse(persisted, out var nextTrigger))
        {
            // No valid persisted time — restart with full interval
            RestartTimerWithFullInterval(interval);
            return;
        }

        var remaining = (nextTrigger - DateTimeOffset.Now).TotalMilliseconds;
        if (remaining <= 0)
        {
            // Missed during sleep → change now, then restart full interval
            Logger.Information("Resume: missed scheduled change (was due {0}), triggering now.", nextTrigger);
            ChangeAllWallpaper();
            RestartTimerWithFullInterval(interval);
        }
        else
        {
            // Still time left → restart with remaining time
            Logger.Information("Resume: next wallpaper change in {0:F0}s.", remaining / 1000);
            _wallpaperTimer.Stop();
            _wallpaperTimer.Interval = remaining;
            _wallpaperTimer.AutoReset = false;
            _wallpaperTimer.Start();
            UpdateNextWallpaperChangeTriggerTime(remaining);
        }
    }

    private static void RestartTimerWithFullInterval(ushort intervalMinutes)
    {
        if (_wallpaperTimer == null) return;
        _wallpaperTimer.Stop();
        _wallpaperTimer.Interval = intervalMinutes * 60 * 1000d;
        _wallpaperTimer.AutoReset = true;
        _wallpaperTimer.Start();
        UpdateNextWallpaperChangeTriggerTime(intervalMinutes);
    }

    private static void OnWallpaperTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            Logger.Information(@"{DateTimeOffset:yyyy-MM-dd HH:mm:ss}: scheduled wallpaper change", DateTimeOffset.Now);
            ChangeAllWallpaper();

            var interval = Properties.Settings.Default.WallpaperChangeInterval;
            // If the first tick was a partial interval, switch to full auto-reset
            if (_wallpaperTimer is { AutoReset: false })
            {
                _wallpaperTimer.Stop();
                _wallpaperTimer.Interval = interval * 60 * 1000d;
                _wallpaperTimer.AutoReset = true;
                _wallpaperTimer.Start();
            }

            UpdateNextWallpaperChangeTriggerTime(interval);
        }
        catch (Exception ex)
        {
            // ignored
            Logger.Error("Scheduling setting wallpaper error: {0}", ex.Message);
        }
    }
}