using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;
using Irvuewin.Helpers.Utils;
using System.Diagnostics;
using Irvuewin.Helpers.AOP;
using Irvuewin.Views;
using static Irvuewin.Helpers.IAppConst;
using Exception = System.Exception;


namespace Irvuewin.ViewModels;

using Serilog;

/// <summary>
/// Channels Manager window<br/>
/// Singleton design.<br/>
/// </summary>
/// <remarks>
/// In memory cached fields:
/// <para>
/// 1. Channel's loaded wallpaper info
/// <code> CacheManager.set(key1=CachedWallpapers, key2=channelId, val=[List]UnsplashPhoto</code>
/// Value item contains only key infos.<br/>
/// </para>
/// <para>
/// 2. Channels's loaded chard index
/// <code>CacheManager.set(key1=CachedWallpaperShard, key2=channelId, val=[int]index</code>
/// This value increases when user scrolling load more photos of a channel.
/// </para>
/// </remarks>
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
    private ChannelViewModel _selectedChannel;

    public ChannelViewModel SelectedChannel
    {
        get => _selectedChannel;
        set
        {
            _selectedChannel = value;
            OnPropertyChanged();
        }
    }

    private bool _allPhotosLoaded;

    /// <summary>
    /// Flag of whether channel's all photos have been loaded
    /// </summary>
    private bool AllPhotosLoaded
    {
        get => _allPhotosLoaded;
        set
        {
            _allPhotosLoaded = value;
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
        CheckedChannelName = Channels
            .First(c => c.Id == _checkedChannelId).Title;
        SelectedChannel = Channels
            .First(c => c.Id == _checkedChannelId);
        foreach (var channel in Channels)
        {
            // Init Channels' photo shard index and photos queue
            if (await FileCacheManager.LoadPhotosAsync(channel.Id) is not { } photos) continue;
            CacheManager.Set(CachedWallpaperShard, channel.Id,
                (photos.Count + PageSize - 1) / PageSize);
            InitChannelPhotos(channel.Id, photos);
        }

        // Load 1st page of checked channel's photos 
        // await LoadPhotos(CheckedChannelId, DefaultQuery);

        // Init Photos
        if (!await LoadPhotos(CheckedChannelId))
            await LoadPhotos(CheckedChannelId, DefaultQuery);
        Logger.Information(@"ChannelsViewModel initialized.");
    }


    /// <summary>
    /// Load Channels info from local cache or web(the first start)<br/>
    /// <para>Loaded data save to Channel, which is window data source.</para>
    /// </summary>
    private async Task LoadChannels()
    {
        var channelIds = SavedChannelIds.Split(',');
        if (FileCacheManager.LoadChannels() is { } cachedChannels
            && cachedChannels.Count == channelIds.Length)
        {
            // Load from disk cache
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
                if (await httpService.GetChannelById(id) is { } channel)
                    Channels.Add(MapperProvider.Mapper.Map<ChannelViewModel>(channel));
            }

            Logger.Information(@"Loaded {ChannelsCount} channels from web.", Channels.Count);
        }
    }


    /// <summary>
    /// Init all cached photos to in-memory cache.
    /// </summary>
    /// <param name="cid">channel id</param>
    /// <param name="photos">Unsplash photo list</param>
    // TODO Filter
    private static void InitChannelPhotos(string cid, List<UnsplashPhoto> photos)
    {
        var cachePhotos = SimplyPhotosQueue(photos);
        CacheManager.Set(CachedWallpapers, cid, cachePhotos);
    }

    /// <summary>
    /// UnsplashPhoto object cached.
    /// </summary>
    /// <param name="photos">unsplash photo list</param>
    /// <returns>Simplified unsplash photo list</returns>
    private static List<UnsplashPhoto> SimplyPhotosQueue(List<UnsplashPhoto> photos)
    {
        var cachePhotos =
            photos.Select(p => new UnsplashPhoto
            {
                Id = p.Id,
                Urls = p.Urls,
                Links = p.Links,
                User = new UnsplashUser()
                {
                    Name = p.User.Name,
                    ProfileImage = p.User.ProfileImage,
                    Links = p.User.Links,
                }
            }).ToList();
        return cachePhotos;
    }

    /// <summary>
    /// Load channel photos from web API and save it to in-memory and local disk cache.
    /// </summary>
    /// <param name="cid">channel id</param>
    /// <param name="query"><see cref="UnsplashQueryParams"/></param>
    /// <returns>True if succeeded</returns>
    public async Task<bool> LoadPhotosShardFromWeb(string cid, UnsplashQueryParams query)
    {
        // web -> in memory cache -> local disk cache
        // load new photos from web API
        var httpService = IHttpClient.GetUnsplashHttpService();
        if (await httpService.GetPhotosOfChannel(cid, query) is not { } photos)
        {
            // API error
            return false;
        }

        if (photos.Count == 0)
        {
            // Sometimes api gets 0 photo from channel
            // Though channel contains photo(s)
            // We assume that all photos are loaded
            AllPhotosLoaded = true;
            // return false;
        }

        ChannelPhotosHandler(cid, photos);
        
        if (photos.Count != 0) await FileCacheManager.CachePhotosAsync(cid, photos);
        return true;
    }

    /// <summary>
    /// Load channel photos from cache.
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    private async Task<bool> LoadPhotos(string cid)
    {
        if (!CacheManager.Exists<List<UnsplashPhoto>>(CachedWallpapers, cid))
        {
            if (await FileCacheManager.LoadPhotosAsync(cid) is { } photos)
            {
                if (photos.Count == 0) return false;
                InitChannelPhotos(cid, photos);
            }
            else return false;
        }

        Photos = [..CacheManager.Get<List<UnsplashPhoto>>(CachedWallpapers, cid)!];
        return true;
    }

    /// <summary>
    /// Load channel wallpapers.<br/>
    /// Ultimate goal of this method is to update <see cref="Photos"/> UI control.
    /// </summary>
    /// <param name="channelId">channel Id</param>
    /// <param name="query"><see cref="UnsplashQueryParams"/></param>
    /// <returns>True if succeeded, False if no photo is loaded.</returns>
    public async Task<bool> LoadPhotos(
        string channelId,
        UnsplashQueryParams query)
    {
        if (!await LoadPhotosShardFromWeb(channelId, query)) return false;
        Photos = [..CacheManager.Get<List<UnsplashPhoto>>(CachedWallpapers, channelId)!];
        return true;
    }

    /// <summary>
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
    }

    /// <summary>
    /// This means user select a new channel from left side listbox to preview its photos.
    /// But next wallpaper will still be fetched from checked channel (Radio).
    /// This will reload all photos preview from selected photo.
    /// </summary>
    /// <param name="item">ChannelViewModel</param>
    private async void OnChannelSelected(ChannelViewModel item)
    {
        try
        {
            SelectedChannel = item;
            if (CacheManager.Exists<List<UnsplashPhoto>>
                    (CachedWallpapers, item.Id))
            {
                await LoadPhotos(item.Id);
            }
            else
            {
                await LoadPhotos(item.Id, DefaultQuery);
            }
        }
        catch (Exception e)
        {
            // ignore
            Logger.Error(@"Channel select error: {0}", e.Message);
        }
    }


    /// <summary>
    /// Once channel is checked, means next wallpaper is fetched from this channel.
    /// </summary>
    /// <param name="item">ChannelViewModel</param>
    private void OnChannelChecked(ChannelViewModel item)
    {
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
            if (!CacheManager.Exists<List<UnsplashPhoto>>(CachedWallpapers, item.Id))
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
            Logger.Information(@"Loading more photos of channel {0}", channel.Id);
            if (IsBusy) return;
            if (AllPhotosLoaded) return;
            IsBusy = true;
            var shard = CacheManager.Get<int>(CachedWallpaperShard, channel.Id);
            UnsplashQueryParams query = new()
            {
                Page = ++shard,
                PerPage = PageSize,
                Orientation = Properties.Settings.Default.WallpaperOrientation
            };
            AllPhotosLoaded = !await LoadPhotos(channel.Id, query);
            IsBusy = false;
            CacheManager.Set(CachedWallpaperShard, channel.Id, shard);
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
    /// <param name="channelId"></param>
    public async Task RefreshPhotos(string channelId)
    {
        // if (channelId is null) return;
        CacheManager.Remove<List<UnsplashPhoto>>(CachedWallpapers, channelId);
        FileCacheManager.UnCacheChannelPhotos(channelId);
        DefaultQuery.Orientation = Properties.Settings.Default.WallpaperOrientation;
        await LoadPhotos(channelId, DefaultQuery);

        // reset
        IrvuewinCore.ResetChannelSequence(channelId);
        CacheManager.Set(CachedWallpaperShard, channelId, 1);
        AllPhotosLoaded = false;
        Logger.Information(@"Refreshed photos for channel {ChannelId}", channelId);
    }

    public async Task RefreshPhotos()
    {
        foreach (var channel in Channels)
        {
            CacheManager.Remove<List<UnsplashPhoto>>(CachedWallpapers, channel.Id);
            FileCacheManager.UnCacheChannelPhotos(channel.Id);
            DefaultQuery.Orientation = Properties.Settings.Default.WallpaperOrientation;

            await LoadPhotosShardFromWeb(channel.Id, DefaultQuery);

            // reset
            IrvuewinCore.ResetChannelSequence(channel.Id);
            CacheManager.Set(CachedWallpaperShard, channel.Id, 1);
            AllPhotosLoaded = false;

            if (channel.Id != SelectedChannel.Id) continue;
            // Set photos
            // Some channels may not contain photos in specify orientation
            var nps = CacheManager.Get<List<UnsplashPhoto>>(CachedWallpapers, SelectedChannel.Id) ?? [];
            Photos = [..nps];
        }
    }

    public void AddChannel(List<UnsplashChannel> selectedChannels)
    {
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
            CacheManager.Set(CachedWallpaperShard, channel.Id, 1);
            Task.Run(() => LoadPhotosShardFromWeb(channel.Id, DefaultQuery));
        }

        IrvuewinCore.AddNewChannelSequence(channelViewModels);
    }

    public void DeleteSelectedChannel(string channelId)
    {
        SavedChannelIds = string.Join(",", SavedChannelIds.Split(",").Where(item => item != channelId));

        Properties.Settings.Default.UserUnsplashChannels = SavedChannelIds;
        Properties.Settings.Default.Save();

        var item = Channels.FirstOrDefault(c => c.Id == channelId);
        if (item != null) Channels.Remove(item);

        // Refresh shard index and wallpaper sequence
        CacheManager.Remove<List<UnsplashPhoto>>(CachedWallpaperShard, channelId);
        IrvuewinCore.DelChannelSequence(channelId);
        // Clear cached memory/disk channel photos
        FileCacheManager.UnCacheChannelPhotos(channelId);

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
            CacheManager.Set(IrvuewinCore.CurrentPointerDisplay.Name, photo.Id);
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
        MessageBoxWindow.Show("?");
    }

    private static void OnHideAuthor(UnsplashPhoto photo)
    {
        MessageBoxWindow.Show("!");
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}