using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;
using Irvuewin.Helpers.Utils;
using System.Diagnostics;
using Irvuewin.Helpers.DB;
using Irvuewin.Helpers.Events;
using Irvuewin.Helpers.HTTP;
using Exception = System.Exception;
using static Irvuewin.Helpers.IAppConst;


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

    private bool _isPhotosEmpty;

    public bool IsPhotosEmpty
    {
        get => _isPhotosEmpty;
        set
        {
            _isPhotosEmpty = value;
            OnPropertyChanged();
        }
    }


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
            SyncPhotosState();
        }
    }

    private void SyncPhotosState()
    {
        IsPhotosEmpty = Photos.Count == 0;
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

    private string CheckedChannelId
    {
        get => _checkedChannelId;
        set
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

    public ICommand ChannelSelected2 { get; }
    public ICommand ChannelChecked2 { get; }
    public ICommand LoadMorePhotos { get; }
    public ICommand SetAsWallpaperCommand { get; }
    public ICommand DownloadPhotoCommand { get; }
    public ICommand ViewPhotoCommand { get; }
    public ICommand ViewAuthorCommand { get; }
    public ICommand HidePhotoCommand { get; }
    public ICommand HideAuthorCommand { get; }
    public ICommand LikePhotoCommand { get; }


    private static readonly Lazy<Task<ChannelsViewModel>> Instance = new(Init);

    private static async Task<ChannelsViewModel> Init()
    {
        var instance = new ChannelsViewModel();
        await instance.InitAsync();
        return instance;
    }

    private ChannelsViewModel()
    {
        LoadMorePhotos = new RelayCommand<UnsplashChannel>(OnLoadMorePhotos);
        ChannelSelected2 = new RelayCommand<ChannelViewModel>(OnChannelSelected);
        ChannelChecked2 = new RelayCommand<ChannelViewModel>(OnChannelChecked);
        SetAsWallpaperCommand = new RelayCommand<UnsplashPhoto>(OnSetAsWallpaper);
        DownloadPhotoCommand = new RelayCommand<UnsplashPhoto>(OnDownloadPhoto);
        ViewPhotoCommand = new RelayCommand<UnsplashPhoto>(OnViewPhoto);
        ViewAuthorCommand = new RelayCommand<UnsplashPhoto>(OnViewAuthor);
        HidePhotoCommand = new RelayCommand<UnsplashPhoto>(OnHidePhoto);
        HideAuthorCommand = new RelayCommand<UnsplashPhoto>(OnHideAuthor);
        LikePhotoCommand = new RelayCommand<UnsplashPhoto>(OnLikePhoto);

        // binding EventBus
        EventBus.WallpapersReplenished += OnWallpaperReplenished;
        EventBus.ChannelSyncCompleted += OnChannelSyncCompleted;
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
        PreviewPhotos(CheckedChannelId, UnsplashQueryParams.Create());
        Logger.Information(@"ChannelsViewModel initialized.");
    }


    /// <summary>
    /// Load Channels info from LiteDB or web(1st startup)<br/>
    /// <para>Loaded data save to Channel, which is window data source.</para>
    /// </summary>
    private async Task LoadChannels()
    {
        var channelIds = SavedChannelIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var cachedChannels = DataBaseService.LoadChannels() ?? [];

        // Ensure Likes channel exists in DB and load it
        var likesDb = cachedChannels.FirstOrDefault(c => c.Id == LikesChannelId);
        if (likesDb == null)
        {
            likesDb = new UnsplashChannel
            {
                Id = LikesChannelId,
                Title = Localization.Instance["Channel_Likes_Title"],
                AllPhotosLoaded = true
            };
            await DataBaseService.UpdateChannel(likesDb);
        }

        List<ChannelViewModel> channelViewModels = [];

        // 1. Add Likes channel at top
        var likesVM = MapperProvider.Mapper.Map<ChannelViewModel>(likesDb);
        likesVM.Title = Localization.Instance["Channel_Likes_Title"];
        likesVM.IsReserved = true;
        likesVM.AllPhotosLoaded = true;
        channelViewModels.Add(likesVM);

        // 2. Add other Unsplash channels
        var httpService = IHttpClient.GetUnsplashHttpService();
        foreach (var id in channelIds)
        {
            var dbChannel = cachedChannels.FirstOrDefault(item => item.Id == id);
            if (dbChannel != null)
            {
                channelViewModels.Add(MapperProvider.Mapper.Map<ChannelViewModel>(dbChannel));
            }
            else
            {
                // Load from web api if missing in DB
                if (await httpService.GetChannelById(id) is not { } channel) continue;
                await DataBaseService.UpdateChannel(channel);
                channelViewModels.Add(MapperProvider.Mapper.Map<ChannelViewModel>(channel));
            }
        }

        Channels = [..channelViewModels];
        foreach (var channel in Channels)
        {
            FastCacheManager.Set(CachedWallpaperPreviewShard, channel.Id, 1);
        }
    }

    /// <summary>
    /// Load channel wallpapers to wallpaper gallery.<br/>
    /// Ultimate goal of this method is to update <see cref="Photos"/> UI control.
    /// </summary>
    /// <param name="channelId">channel Id</param>
    /// <param name="query"><see cref="UnsplashQueryParams"/></param>
    /// <param name="append">append items to Photos list or not. default no</param>
    /// <returns>True if succeeded, False if no photo is loaded.</returns>
    private bool PreviewPhotos(string channelId, UnsplashQueryParams query, bool append = false)
    {
        var skip = append ? Photos.Count : 0;
        var take = query.GetPerPage();

        // 获取频道最新状态，判断网络端是否已经全部拉取完毕
        var dbChannel = DataBaseService.GetChannel(channelId);
        var isAllLoaded = dbChannel?.AllPhotosLoaded ?? false;

        if (DataBaseService.LoadPhotosByOffset(channelId, skip, take)
            is not { Count: > 0 } photos)
        {
            // 如果本地数据没读到，且网络端还没到底，再去请求拉取壁纸
            if (!isAllLoaded && channelId != LikesChannelId)
            {
                EventBus.PublishPoolLow(channelId);
            }
            SyncPhotosState();
            return false;
        }


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

        // 如果本次加载的数量偏少，试图去预加载，但前提是网络端还有剩余数据
        if (photos.Count < PageSize / 2 && !isAllLoaded && channelId != LikesChannelId)
        {
            EventBus.PublishPoolLow(channelId);
        }
        SyncPhotosState();
        return true;
    }

    /// <summary>
    /// This means user select a new channel from left side listbox to preview its photos.
    /// But next wallpaper will still be fetched from checked channel (Radio).
    /// This will reload all photos preview from selected photo.
    /// </summary>
    /// <param name="item">ChannelViewModel</param>
    private void OnChannelSelected(ChannelViewModel? item)
    {
        if (item == null) return;
        try
        {
            // reset previewShard
            if (SelectedChannel != null)
            {
                FastCacheManager.Set(CachedWallpaperPreviewShard, SelectedChannel.Id, 1);
            }

            FastCacheManager.Set(CachedWallpaperPreviewShard, item.Id, 1);
            SelectedChannel = item;
            Photos.Clear();
            PreviewPhotos(item.Id, UnsplashQueryParams.Create());
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
    private void OnLoadMorePhotos(UnsplashChannel channel)
    {
        try
        {
            if (IsBusy) return;
            IsBusy = true;
            if (!FastCacheManager.TryGet(CachedWallpaperPreviewShard, channel.Id, out int previewShard)) 
            {
                IsBusy = false;
                return;
            }

            var dbChannel = DataBaseService.GetChannel(channel.Id);
            if (dbChannel is { AllPhotosLoaded: true })
            {
                // all photos loaded to LiteDB
                // Expert to access more photos, that's not possible
                if (Photos.Count >= DataBaseService.LoadedPhotosCountExcluded(channel.Id))
                {
                    IsBusy = false;
                    return;
                }
            }
            
            var query = UnsplashQueryParams.Create().Page(previewShard++);

            Logger.Information(@"Loading more photos of channel {0},  UI virtual shard {1}", channel.Id, query.GetPage());
            if (PreviewPhotos(channel.Id, query, true))
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
            IsBusy = false;
        }
    }

    private async void OnLikePhoto(UnsplashPhoto photo)
    {
        try
        {
            var isLiked = !photo.IsLiked;
            await DataBaseService.UpdatePhotoLikedStatus(photo.Id, isLiked);
            
            // Sync current instance
            photo.IsLiked = isLiked;
            photo.LikedAt = isLiked ? DateTimeOffset.UtcNow.Ticks : 0;

            if (SelectedChannel?.Id == LikesChannelId)
            {
                if (!isLiked)
                {
                    Photos.Remove(photo);
                    SyncPhotosState();
                }
                else
                {
                    // If somehow we liked a photo while in Likes channel (though unlikely unless we have a 'recommendation' or something)
                    // we might want to refresh. But usually you like from OTHER channels.
                    Photos.Insert(0, photo);
                    SyncPhotosState();
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Like photo error");
        }
    }

    /// <summary>
    /// Refresh channel photos cache.<br/>
    /// Triggered when user change wallpaper filter rules. 
    /// </summary>
    /// <param name="channel"></param>
    public Task RefreshPhotos(UnsplashChannel channel)
    {
        if (channel.Id == LikesChannelId) return Task.CompletedTask; // Liked channel do nothing
        IsBusy = true;
        EventBus.PublishForceSync(channel.Id);
        Logger.Information(@"Dispatched force sync for channel {ChannelId}", channel.Id);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Invoked by user change system settings.
    /// </summary>
    public async Task RefreshPhotos()
    {
        foreach (var channel in Channels)
        {
            if (channel.Id == LikesChannelId) continue;
            // DataBaseService.RemoveChannelPhotos(channel.Id);
            EventBus.PublishPoolLow(channel.Id);

            // reset channel sequence
            channel.Sequence = 1;
            channel.Shard = 1; // 重置壁纸池的分片页码为首页
            channel.AllPhotosLoaded = false;
            await DataBaseService.UpdateChannel(channel);
            FastCacheManager.Set(CachedWallpaperPreviewShard, channel.Id, 1);

            if (channel.Id != SelectedChannel!.Id) continue;
            // Set photos
            // Some channels may not contain photos in specify orientation
            Photos.Clear();
            PreviewPhotos(channel.Id, UnsplashQueryParams.Create());
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
            // Task.Run(() => LoadPhotosShardFromWeb(channel.Id, DefaultQuery));
            EventBus.PublishPoolLow(channel.Id);
        }

        IrvuewinCore.AddNewChannel(channelViewModels);
    }

    public void DeleteSelectedChannel(string channelId)
    {
        var item = Channels.FirstOrDefault(c => c.Id == channelId);
        if (item == null || item.IsReserved) return;

        SavedChannelIds = string.Join(",", SavedChannelIds.Split(",").Where(id => id != channelId));


        Properties.Settings.Default.UserUnsplashChannels = SavedChannelIds;
        Properties.Settings.Default.Save();

        Channels.Remove(item);


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
            EventBus.PublishWallpaperChanged(IrvuewinCore.CurrentPointerDisplay.Name, photo.Id);
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
        var attrUrl = string.Concat(photo.Links.Html.ToString(), Attribution);
        // Logger.Information("View wallpaper on unsplash: {0}", attrUrl);
        ICommonCommands.OpenUrl(attrUrl);
    }

    private static void OnViewAuthor(UnsplashPhoto photo)
    {
        var attrUrl = string.Concat(photo.User.Links.Html.ToString(), Attribution);
        // Logger.Information("View author: {0}", attrUrl);
        ICommonCommands.OpenUrl(attrUrl);
    }


    private void OnHidePhoto(UnsplashPhoto photo)
    {
        photo.IsHidden = true;
        DataBaseService.UpdatePhoto(photo);
        Photos.Remove(photo);
        SyncPhotosState();
    }

    private async void OnHideAuthor(UnsplashPhoto photo)
    {
        try
        {
            var username = photo.User.Username;
            if (string.IsNullOrWhiteSpace(username)) return;

            var currentList = Properties.Settings.Default.UserFilterList ?? "";
            var users = currentList.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            if (!users.Contains(username))
            {
                users.Add(username);
                Properties.Settings.Default.UserFilterList = string.Join(",", users);
                Properties.Settings.Default.Save();
                Logger.Information(@"Added author {0} to filter list.", username);
            }

            // Sync Database
            await Task.Run(() => DataBaseService.BlockAuthor(username));

            // Sync UI without full refresh
            var toRemove = Photos.Where(p => p.User.Username == username).ToList();
            foreach (var p in toRemove)
            {
                Photos.Remove(p);
                SyncPhotosState();
            }
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

    private void OnWallpaperReplenished()
    {
        // 必须切回主线程，否则操作 UI 绑定的集合会报跨线程操作异常
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            // 如果用户还在看某个频道，利用现成的翻页逻辑，继续硬着头皮往下翻一页
            if (SelectedChannel != null)
            {
                OnLoadMorePhotos(SelectedChannel);
            }
        });
    }

    private void OnChannelSyncCompleted(string channelId)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (SelectedChannel == null || SelectedChannel.Id != channelId) return;
            IsBusy = false;
            Photos.Clear();
            PreviewPhotos(channelId, UnsplashQueryParams.Create());
        });
    }
}