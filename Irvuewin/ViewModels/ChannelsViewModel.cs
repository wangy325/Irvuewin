using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;
using Irvuewin.Helpers.Utils;
using System.Diagnostics;
using Irvuewin.Helpers.DB;
using Irvuewin.Views;
using static Irvuewin.Helpers.IAppConst;
using Exception = System.Exception;


namespace Irvuewin.ViewModels;

using Serilog;

/// <summary>
/// Channels Manager window<br/>
/// Singleton design.<br/>
/// </summary>
public class ChannelsViewModel : INotifyPropertyChanged
{
    private static readonly ILogger Logger = Log.ForContext<ChannelsViewModel>();

    public event PropertyChangedEventHandler? PropertyChanged;

    private string SavedChannelIds { get; set; } = Properties.Settings.Default.UserUnsplashChannels;

    private ObservableCollection<UnsplashPhoto> _photos = [];

    /// <summary>
    /// channel manage window datasource
    /// </summary>
    public ObservableCollection<UnsplashPhoto> Photos
    {
        get => _photos;
        private set
        {
            _photos = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<ChannelViewModel> _channels = [];

    /// <summary>
    /// channel manage window data source
    /// </summary>
    public ObservableCollection<ChannelViewModel> Channels
    {
        get => _channels;
        private set
        {
            _channels = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Checked channel which new wallpaper is fetched from
    /// </summary>
    private string _checkedChannelId = Properties.Settings.Default.UserCheckedChannel;

    public string CheckedChannelId
    {
        get => _checkedChannelId;
        private set
        {
            _checkedChannelId = value;
            OnPropertyChanged();
        }
    }

    private string _checkedChannelName = "";

    public string CheckedChannelName
    {
        get => _checkedChannelName;
        private set
        {
            _checkedChannelName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Channel is selected to show wallpaper preview.
    /// </summary>
    private ChannelViewModel? _selectedChannel;

    public ChannelViewModel? SelectedChannel
    {
        get => _selectedChannel;
        set
        {
            _selectedChannel = value;
            OnPropertyChanged();
        }
    }

    // private int ShardIndex { get; set; } = 1;
    private bool IsBusy { get; set; }
    private UnsplashQueryParams DefaultQuery { get; set; }

    public ICommand ChannelSelected2 { get; }
    public ICommand ChannelChecked2 { get; }
    public ICommand LoadMorePhotos { get; }
    public ICommand SetAsWallpaperCommand { get; }
    public ICommand DownloadPhotoCommand { get; }
    public ICommand ViewPhotoCommand { get; }
    public ICommand ViewAuthorCommand { get; }
    public ICommand HidePhotoCommand { get; }
    public ICommand HideAuthorCommand { get; }


    private static readonly Lazy<Task<ChannelsViewModel>> Instance = new(Init);

    private static async Task<ChannelsViewModel> Init()
    {
        var instance = new ChannelsViewModel();
        await instance.InitAsync();
        return instance;
    }

    private ChannelsViewModel()
    {
        DefaultQuery = new UnsplashQueryParams
        {
            Page = 1,
            PerPage = PageSize,
            Orientation = Properties.Settings.Default.WallpaperOrientation
        };
        LoadMorePhotos = new RelayCommand<UnsplashChannel>(OnLoadMorePhotos);
        ChannelSelected2 = new RelayCommand<ChannelViewModel>(OnChannelSelected);
        ChannelChecked2 = new RelayCommand<ChannelViewModel>(OnChannelChecked);
        SetAsWallpaperCommand = new RelayCommand<UnsplashPhoto>(OnSetAsWallpaper);
        DownloadPhotoCommand = new RelayCommand<UnsplashPhoto>(OnDownloadPhoto);
        ViewPhotoCommand = new RelayCommand<UnsplashPhoto>(OnViewPhoto);
        ViewAuthorCommand = new RelayCommand<UnsplashPhoto>(OnViewAuthor);
        HidePhotoCommand = new RelayCommand<UnsplashPhoto>(OnHidePhoto);
        HideAuthorCommand = new RelayCommand<UnsplashPhoto>(OnHideAuthor);
    }

    public static Task<ChannelsViewModel> GetInstanceAsync()
    {
        return Instance.Value;
    }

    /// <summary>
    /// Get instance after initialized. Or will be blocked.
    /// </summary>
    /// <returns></returns>
    public static ChannelsViewModel GetInstance()
    {
        return Instance.Value.Result;
    }

    private async Task InitAsync()
    {
        // Channels
        await LoadChannels();
        Channels.First(c => c.Id == _checkedChannelId).IsChecked = true;
        // selected = checked on startup
        CheckedChannelName = Channels
            .First(c => c.Id == _checkedChannelId).Title;
        SelectedChannel = Channels
            .First(c => c.Id == _checkedChannelId);

        // Init checked channel Photos
        await LoadPhotos(CheckedChannelId, DefaultQuery);
        Logger.Information(@"ChannelsViewModel initialized.");
    }


    /// <summary>
    /// Load Channels info from LiteDB or web(1st startup)<br/>
    /// <para>Loaded data save to Channel, which is window data source.</para>
    /// </summary>
    private async Task LoadChannels()
    {
        var channelIds = SavedChannelIds.Split(',');
        if (DataBaseService.LoadChannels() is { } cachedChannels
            && cachedChannels.Count == channelIds.Length)
        {
            // Load from LiteDB
            List<ChannelViewModel> channelViewModels = [];
            channelViewModels.AddRange(
                cachedChannels.Select(item => MapperProvider.Mapper.Map<ChannelViewModel>(item)));
            Channels = [..channelViewModels];
        }
        else
        {
            // Load from web api
            var httpService = IHttpClient.GetUnsplashHttpService();
            foreach (var id in channelIds)
            {
                if (await httpService.GetChannelById(id) is not { } channel) continue;
                await DataBaseService.UpdateChannel(channel);
                Channels.Add(MapperProvider.Mapper.Map<ChannelViewModel>(channel));
                // Init channel's photo preview shard index to FastCache
                FastCacheManager.Set(CachedWallpaperPreviewShard, id, 1);
            }

            Logger.Information(@"Loaded {ChannelsCount} channels from web.", Channels.Count);
        }
    }

    /// <summary>
    /// Load channel photos from web API and save it to LiteDB.
    /// </summary>
    /// <param name="cid">channel id</param>
    /// <param name="query"><see cref="UnsplashQueryParams"/></param>
    /// <returns>True if succeeded</returns>
    public async Task<bool> LoadPhotosShardFromWeb(string cid, UnsplashQueryParams query)
    {
        // web  -> LiteDB
        // load new photos from web API
        var channel = DataBaseService.GetChannel(cid)!;
        if (channel.AllPhotosLoaded) return false;
        var httpService = IHttpClient.GetUnsplashHttpService();
        if (await httpService.GetPhotosOfChannel(cid, query) is not { } photos)
        {
            // API error
            return false;
        }

        // Update channel's shard and load flag if necessary
        if (photos.Count == 0)
        {
            // Sometimes api gets 0 photo from channel
            // Though channel contains photo(s)
            // We assume that all photos are loaded
            channel.AllPhotosLoaded = true;
            await DataBaseService.UpdateChannel(channel);
            return false;
        }

        await DataBaseService.CachePhotos(cid, photos);
        // Ignore refresh channel's shard update
        if (query.PerPage != PageSize) return true;
        channel.Shard = query.Page;
        await DataBaseService.UpdateChannel(channel);

        return true;
    }

    /// <summary>
    /// Load channel wallpapers.<br/>
    /// Ultimate goal of this method is to update <see cref="Photos"/> UI control.
    /// </summary>
    /// <param name="channelId">channel Id</param>
    /// <param name="query"><see cref="UnsplashQueryParams"/></param>
    /// <param name="append">append items to Photos list or not. default no</param>
    /// <returns>True if succeeded, False if no photo is loaded.</returns>
    public async Task<bool> LoadPhotos(string channelId, UnsplashQueryParams query, bool append = false)
    {
        var leastCount = query.Page * query.PerPage;
        if (await DataBaseService.LoadPhotoCount(channelId) < leastCount)
        {
            // load from web and save to LiteDB
            if (!await LoadPhotosShardFromWeb(channelId, query)) return false;
        }

        if (await DataBaseService.LoadPhotosShardAsync(channelId, query)
            is not { Count: > 0 } photos) return false;
        if (append)
        {
            var tmpl = Photos.ToList();
            tmpl.AddRange(photos);
            Photos = [..tmpl];
        }
        else
        {
            Photos = [..photos];
        }

        return true;
    }

    /*/// <summary>
    /// Handler of raw channel photos.
    /// </summary>
    /// <param name="channelId">channel id</param>
    /// <param name="photos">raw photo list</param>
    [FilterByBlockList]
    [FilterBySize(MinWidth = 1920, MinHeight = 1080)]
    private static void ChannelPhotosHandler(
        string channelId,
        List<UnsplashPhoto> photos)
    {
        var cachePhotos = SimplyPhotosQueue(photos);

        if (!CacheManager.TryGet<List<UnsplashPhoto>?>
                (CachedWallpapers, channelId, out var val)
            || val is null)
        {
            CacheManager.Set(CachedWallpapers, channelId, cachePhotos);
        }
        else
        {
            val.AddRange(cachePhotos);
            CacheManager.Set(CachedWallpapers, channelId, val);
        }
    }*/

    /// <summary>
    /// This means user select a new channel from left side listbox to preview its photos.
    /// But next wallpaper will still be fetched from checked channel (Radio).
    /// This will reload all photos preview from selected photo.
    /// </summary>
    /// <param name="item">ChannelViewModel</param>
    private async void OnChannelSelected(ChannelViewModel? item)
    {
        if (item == null) return;
        try
        {
            // reset previewShard
            FastCacheManager.Set(CachedWallpaperPreviewShard, SelectedChannel!.Id, 1);
            SelectedChannel = item;
            Photos.Clear();
            await LoadPhotos(item.Id, DefaultQuery);
        }
        catch (Exception e)
        {
            Logger.Error(@"Channel select error: {0}", e.Message);
        }
    }


    /// <summary>
    /// Once channel is checked, means next wallpaper is fetched from this channel.
    /// </summary>
    /// <param name="item">ChannelViewModel</param>
    private void OnChannelChecked(ChannelViewModel? item)
    {
        if (item == null) return;
        try
        {
            // Update selected status
            item.IsChecked = true;
            // Ignore continuous click
            if (CheckedChannelId == item.Id) return;
            CheckedChannelId = item.Id;
            CheckedChannelName = item.Title;
            Channels.Where(c => c != item)
                .ToList()
                .ForEach(c => c.IsChecked = false);
            // Once app startup at 1st time
            // And user checked channel from tray menu
            // Channel's photo is not cached
            // We need to preload photos
            Task.Run(() => LoadPhotosShardFromWeb(item.Id, DefaultQuery));

            // Update selected index to settings
            Properties.Settings.Default.UserCheckedChannel = CheckedChannelId;
            Properties.Settings.Default.Save();
            Logger.Information(@"Checked Channel saved: {0}", CheckedChannelId);
        }
        catch (Exception e)
        {
            // ignore
            Logger.Error(@"Channel check error: {0}", e.Message);
        }
    }

    /// <summary>
    /// Wallpaper view board scroll update.
    /// </summary>
    /// <param name="channel">UnsplashChannel</param>
    private async void OnLoadMorePhotos(UnsplashChannel channel)
    {
        try
        {
            if (IsBusy) return;
            IsBusy = true;
            if (!FastCacheManager.TryGet(CachedWallpaperPreviewShard, channel.Id, out int previewShard)) return;


            if (channel.AllPhotosLoaded)
            {
                // all photos loaded to LiteDB
                // Expert to access more photos, that's not possible
                if (previewShard * PageSize > await DataBaseService.LoadPhotoCount(channel.Id)) return;
            }

            UnsplashQueryParams query = new()
            {
                Page = ++previewShard,
                PerPage = PageSize,
                Orientation = Properties.Settings.Default.WallpaperOrientation
            };

            Logger.Information(@"Loading more photos of channel {0},  shard {1}", channel.Id, query.Page);
            if (await LoadPhotos(channel.Id, query, true))
            {
                // Update previewShard
                FastCacheManager.Set(CachedWallpaperPreviewShard, channel.Id, previewShard);
            }

            IsBusy = false;
        }
        catch (Exception e)
        {
            // ignore
            Logger.Error(e, "Load more photos error");
        }
    }

    /// <summary>
    /// Refresh channel photos cache.<br/>
    /// Triggered when user change wallpaper filter rules. 
    /// </summary>
    /// <param name="channel"></param>
    public async Task RefreshPhotos(UnsplashChannel channel)
    {
        // 劣势: 如果sequence已经很大的情况下,
        // 如果不重置sequence，刷新所有图片的话,
        // 性能会下降
        var shard = channel.Shard;
        UnsplashQueryParams query = new()
        {
            Page = 1,
            PerPage = shard * PageSize,
            Orientation = Properties.Settings.Default.WallpaperOrientation
        };
        await LoadPhotosShardFromWeb(channel.Id, query);
        Logger.Information(@"Refreshed photos for channel {ChannelId}", channel.Id);
    }

    /// <summary>
    /// Invoked by user change system settings.
    /// </summary>
    public async Task RefreshPhotos()
    {
        foreach (var channel in Channels)
        {
            await DataBaseService.RemoveChannelPhotos(channel.Id);
            DefaultQuery.Orientation = Properties.Settings.Default.WallpaperOrientation;

            await LoadPhotosShardFromWeb(channel.Id, DefaultQuery);

            // reset channel sequence
            channel.Sequence = 1;
            channel.AllPhotosLoaded = false;
            await DataBaseService.UpdateChannel(channel);
            FastCacheManager.Set(CachedWallpaperPreviewShard, channel.Id, 1);

            if (channel.Id != SelectedChannel!.Id) continue;
            // Set photos
            // Some channels may not contain photos in specify orientation
            var nps = await DataBaseService.LoadPhotosShardAsync(channel.Id, DefaultQuery);
            Photos = [..nps];
        }
    }

    public void AddChannel(List<UnsplashChannel> selectedChannels)
    {
        DataBaseService.CacheChannels(selectedChannels);
        var newChannels = string.Join(",", selectedChannels.Select(channel => channel.Id));
        SavedChannelIds = SavedChannelIds + "," + newChannels;
        // System settings
        Properties.Settings.Default.UserUnsplashChannels = SavedChannelIds;
        Properties.Settings.Default.Save();

        // Map to ChannelViewModel list
        var cvm = selectedChannels.Select(channel => MapperProvider.Mapper.Map<ChannelViewModel>(channel));
        var channelViewModels = cvm.ToList();
        Channels = [..Channels.Concat(channelViewModels).ToList()];
        Logger.Information(@"New Channels: {Join}", string.Join(",", Channels.Select(channel => channel.Id)));

        // Update shard index and wallpaper sequence
        foreach (var channel in selectedChannels)
        {
            Task.Run(() => LoadPhotosShardFromWeb(channel.Id, DefaultQuery));
        }

        IrvuewinCore.AddNewChannel(channelViewModels);
    }

    public void DeleteSelectedChannel(string channelId)
    {
        SavedChannelIds = string.Join(",", SavedChannelIds.Split(",").Where(item => item != channelId));

        Properties.Settings.Default.UserUnsplashChannels = SavedChannelIds;
        Properties.Settings.Default.Save();

        var item = Channels.FirstOrDefault(c => c.Id == channelId);
        if (item != null) Channels.Remove(item);

        // Refresh shard index and wallpaper sequence
        IrvuewinCore.DelChannel(channelId);
        // Clear cached memory/disk channel photos
        DataBaseService.RemoveChannel(channelId);

        if (channelId != CheckedChannelId)
        {
            SelectedChannel = Channels.First(c => c.Id == CheckedChannelId);
        }
        else
        {
            // reset checked channel to default
            CheckedChannelId = Channels[0].Id;
            Channels.First(c => c.Id == CheckedChannelId).IsChecked = true;
            SelectedChannel = Channels.First(c => c.Id == CheckedChannelId);
        }
    }

    private static async void OnSetAsWallpaper(UnsplashPhoto photo)
    {
        try
        {
            IrvuewinCore.CheckPointer();
            if (await WallpaperUtil.SetWallpaperForSpecificMonitor(IrvuewinCore.CurrentPointerDisplay, photo) is not
                { } path) return;
            IrvuewinCore.UpdateDisplayWallpaperStack(IrvuewinCore.CurrentPointerDisplay.Name, path);
            FastCacheManager.Set(IrvuewinCore.CurrentPointerDisplay.Name, photo.Id);
            IrvuewinCore.BroadcastWallpaperChanged(IrvuewinCore.CurrentPointerDisplay.Name, photo.Id);
        }
        catch (Exception e)
        {
            // ignore
            Logger.Error(@"Manually setup wallpaper error: {0}", e.Message);
        }
    }

    private static async void OnDownloadPhoto(UnsplashPhoto photo)
    {
        try
        {
            var dest = Properties.Settings.Default.WallpaperSavedPath;
            if (string.IsNullOrWhiteSpace(dest))
            {
                dest = DefaultWallpaperDownloadDir;
            }

            // This will pre-download wallpaper
            var path = await WallpaperUtil.GetWallpaperFullPath(photo, null);
            FileUtils.CopyFileToDir(path, dest);
            var openFolder = Properties.Settings.Default.OpenSavedWallpaper;
            if (openFolder)
            {
                Process.Start("explorer.exe", dest);
            }
        }
        catch (Exception e)
        {
            // ignore
            Logger.Error(@"Manually download wallpaper error: {0}", e.Message);
        }
    }

    private static void OnViewPhoto(UnsplashPhoto photo)
    {
        ICommonCommands.OpenUrl(photo.Links.Html.ToString());
    }

    private static void OnViewAuthor(UnsplashPhoto photo)
    {
        ICommonCommands.OpenUrl(photo.User.Links.Html.ToString());
    }


    private static void OnHidePhoto(UnsplashPhoto photo)
    {
        photo.IsFiltered = true;
        DataBaseService.UpdatePhoto(photo);
    }

    private static async void OnHideAuthor(UnsplashPhoto photo)
    {
        try
        {
            var username = photo.User.Username;
            if (string.IsNullOrWhiteSpace(username)) return;

            var currentList = Properties.Settings.Default.UserFilterList ?? "";
            var users = currentList.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            if (users.Contains(username)) return;
            users.Add(username);
            Properties.Settings.Default.UserFilterList = string.Join(",", users);
            Properties.Settings.Default.Save();
            Logger.Information(@"Added author {0} to filter list.", username);

            await (await GetInstanceAsync()).RefreshPhotos();
        }
        catch (Exception e)
        {
            Logger.Error(@"Hide author error: {0}", e.Message);
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}