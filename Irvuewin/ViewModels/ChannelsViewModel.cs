using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Models;
using Irvuewin.Models.Unsplash;
using Exception = System.Exception;


namespace Irvuewin.ViewModels;
using Serilog;

// Singleton
public class ChannelsViewModel : INotifyPropertyChanged
{
    private static readonly ILogger Logger = Log.ForContext(typeof(ChannelsViewModel));
    public event PropertyChangedEventHandler? PropertyChanged;

    private string SavedChannels { get; set; } = Properties.Settings.Default.UserUnsplashChannels;
    private ObservableCollection<UnsplashPhoto> _photos = [];

    public ObservableCollection<UnsplashPhoto> Photos
    {
        get => _photos;
        set
        {
            _photos = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<ChannelViewModel> _channels = [];

    public ObservableCollection<ChannelViewModel> Channels
    {
        get => _channels;
        set
        {
            _channels = value;
            OnPropertyChanged();
        }
    }

    private string _checkedChannel = Properties.Settings.Default.UserCheckedChannel;

    public string CheckedChannel
    {
        get => _checkedChannel;
        set
        {
            _checkedChannel = value;
            OnPropertyChanged();
        }
    }

    private string _checkedChannelName = "";

    public string CheckedChannelName
    {
        get => _checkedChannelName;
        set
        {
            _checkedChannelName = value;
            OnPropertyChanged();
        }
    }

    // Wallpaper shard index for each channel
    private readonly Dictionary<string, int> _shardIndex = new();


    // Loaded photos for each channel (dynamic update)
    public Dictionary<string, int> LoadedPhotoCount { get; set; } = [];

    private bool _allPhotosLoaded;

    public bool AllPhotosLoaded
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

    public ICommand ChannelSelected2 { get; set; }
    public ICommand ChannelChecked2 { get; set; }
    public ICommand LoadMorePhotos { get; set; }

    private static readonly Lazy<Task<ChannelsViewModel>> Instance = new(Init);

    private static async Task<ChannelsViewModel> Init()
    {
        var instance = new ChannelsViewModel();
        await instance.InitAsync();
        return instance;
    }

    private ChannelsViewModel()
    {
        // _selectedChannel = new ChannelViewModel(); // useless
        DefaultQuery = new UnsplashQueryParams
        {
            Page = 1,
            PerPage = IAppConst.PageSize,
            Orientation = Properties.Settings.Default.WallpaperOrientation
        };
        LoadMorePhotos = new RelayCommand<object>(OnLoadMorePhotos);
        ChannelSelected2 = new RelayCommand<ChannelViewModel>(OnChannelSelected);
        ChannelChecked2 = new RelayCommand<ChannelViewModel>(OnChannelChecked);
    }

    public static Task<ChannelsViewModel> GetInstanceAsync()
    {
        // TODO 优化初始化过程
        return Instance.Value;
    }

    public static ChannelsViewModel GetInstance()
    {
        return Instance.Value.Result;
    }

    private async Task InitAsync()
    {
        // Channels
        await LoadChannels();
        Channels.First(c => c.Id == _checkedChannel).IsChecked = true;
        CheckedChannelName = Channels.First(c => c.Id == _checkedChannel).Title;
        // Init Channels photo shard index
        foreach (var channel in Channels)
        {
            _shardIndex[channel.Id] = 1;
            LoadedPhotoCount[channel.Id] = await UnsplashCache.LoadPhotoCountAsync(channel.Id);
        }
        
        // Load 1st page of channel's photos 
        await LoadPhotos(CheckedChannel, DefaultQuery);
        Logger.Information(@"=========> ChannelsViewModel initialized.");
    }


    private async Task LoadChannels()
    {
        var channelIds = SavedChannels.Split(',');
        // Load from disk cache
        if (await UnsplashCache.LoadChannelsAsync() is { } cachedChannels
            && cachedChannels.Any()
            && cachedChannels.Count == channelIds.Length)
        {
            List<ChannelViewModel> channelViewModels = [];
            channelViewModels.AddRange(
                cachedChannels.Select(item => MapperProvider.Mapper.Map<ChannelViewModel>(item)));
            Channels = [..channelViewModels];
        }
        // Load from web api
        else
        {
            var httpService = IHttpClient.GetUnsplashHttpService();
            foreach (var id in channelIds)
            {
                if (await httpService.GetChannelById(id) is { } channel)
                    Channels.Add(MapperProvider.Mapper.Map<ChannelViewModel>(channel));
            }
            Logger.Information(@"Loaded {ChannelsCount} channels from web.", Channels.Count);
            // cache channels
            // await CacheChannels();
        }
    }

    private async Task<bool> LoadPhotos(string channelId, UnsplashQueryParams query, bool append = false)
    {
        var cacheIndex = new PhotosCachePageIndex
        {
            ChannelId = channelId,
            PageIndex = query.Page
        };
        if (await UnsplashCache.LoadPhotosShardAsync(cacheIndex) is { } cachedPhotos
            && cachedPhotos.Any())
            // load from cache
        {
            RenewChannelPhotos(cachedPhotos, append);
            return true;
        }

        // Load from unsplash web API
        var httpService = IHttpClient.GetUnsplashHttpService();
        if (await httpService.GetPhotosOfChannel(channelId, query) is { } photos)
        {
            // Sometimes api can not get photos of a channel
            // Though channel contains photo(s)
            if (!photos.Any() && append)
            {
                return false;
            }

            RenewChannelPhotos(photos, append);
            // update cache
            if (photos.Any()) await CachePhotos(cacheIndex, photos);
            LoadedPhotoCount[channelId] = Photos.Count;
            return true;
        }

        return false;
    }

    private void RenewChannelPhotos(List<UnsplashPhoto> photos, bool append)
    {
        if (append)
        {
            foreach (var item in photos)
            {
                Photos.Add(item);
            }
        }
        else
        {
            // Discard previous photos
            Photos = [..photos];
        }
    }

    /// <summary>
    /// This means user select a new channel from left side listbox to preview its photos.
    /// But next wallpaper will still be fetched from checked channel (Radio).
    /// This will reload all photos preview from selected photo.
    /// </summary>
    /// <param name="param"></param>
    private async void OnChannelSelected(object param)
    {
        try
        {
            if (param is not ChannelViewModel item) return;
            // Reset photos loaded status and shard index
            AllPhotosLoaded = false;
            foreach (var key in _shardIndex.Keys.ToList())
            {
                _shardIndex[key] = 1;
            }

            LoadedPhotoCount[item.Id] = await UnsplashCache.LoadPhotoCountAsync(item.Id);

            // Load photos
            DefaultQuery.Orientation = Properties.Settings.Default.WallpaperOrientation;
            await LoadPhotos(item.Id, DefaultQuery);
        }
        catch (Exception e)
        {
            // ignore
        }
    }


    /// <summary>
    /// Once channel is checked, means next wallpaper is fetched from this channel.
    /// </summary>
    /// <param name="param"></param>
    private void OnChannelChecked(object param)
    {
        try
        {
            if (param is not ChannelViewModel item) return;
            
            // Update selected status
            item.IsChecked = true;
            // Ignore continuous click
            if (CheckedChannel == item.Id) return;
            CheckedChannel = item.Id;
            CheckedChannelName = item.Title;
            Channels.Where(c => c != item)
                .ToList()
                .ForEach(c => c.IsChecked = false);


            // Update selected index to settings
            Properties.Settings.Default.UserCheckedChannel = CheckedChannel;
            Properties.Settings.Default.Save();
            Logger.Information(@"Checked Channel saved: {S}", CheckedChannel);
        }
        catch (Exception e)
        {
            // ignore
        }
    }

    private async void OnLoadMorePhotos(object obj)
    {
        if (IsBusy) return;
        if (AllPhotosLoaded) return;
        IsBusy = true;
        _shardIndex[CheckedChannel]++;
        try
        {
            UnsplashQueryParams query = new()
            {
                Page = _shardIndex[CheckedChannel],
                PerPage = IAppConst.PageSize,
                Orientation = Properties.Settings.Default.WallpaperOrientation
            };
            AllPhotosLoaded = !await LoadPhotos(CheckedChannel, query, true);
            IsBusy = false;
        }
        catch (Exception e)
        {
            // ignore
            Logger.Error(e, "Load more photos error");
        }
    }

    public async Task RefreshPhotos(string channelId)
    {
        var httpService = IHttpClient.GetUnsplashHttpService();
        UnCachePhotos(channelId);
        DefaultQuery.Orientation = Properties.Settings.Default.WallpaperOrientation;
        if (await httpService.GetPhotosOfChannel(channelId, DefaultQuery) is { } photos
            && photos.Count != 0)
        {
            Photos = [..photos];
            // update cache
            var cacheIndex = new PhotosCachePageIndex
            {
                ChannelId = channelId,
                PageIndex = DefaultQuery.Page
            };
            await CachePhotos(cacheIndex, photos);
        }

        // reset
        TrayMenuHelper.ResetChannelSequence(channelId);
        _shardIndex[channelId] = 1;
        AllPhotosLoaded = false;
        // reset photos count
        LoadedPhotoCount[channelId] = Photos.Count;
        Logger.Information(@"Refreshed photos for channel {ChannelId}", channelId);
    }

    public async Task CacheChannels()
    {
        await UnsplashCache.CacheChannelsAsync([..Channels]);
    }

    public async Task CachePhotos(PhotosCachePageIndex cachePageIndex, List<UnsplashPhoto> photos)
    {
        await UnsplashCache.CachePhotosAsync(cachePageIndex, photos);
    }

    public void UnCachePhotos(string channelId)
    {
        UnsplashCache.UnCacheChannelPhotos(channelId);
    }

    public void AddChannel(List<UnsplashChannel> selectedChannels)
    {
        var newChannels = string.Join(",", selectedChannels.Select(channel => channel.Id));
        SavedChannels = SavedChannels + "," + newChannels;
        // System settings
        Properties.Settings.Default.UserUnsplashChannels = SavedChannels;
        Properties.Settings.Default.Save();

        // Map to ChannelViewModel list
        var cvm = selectedChannels.Select(channel => MapperProvider.Mapper.Map<ChannelViewModel>(channel));
        var channelViewModels = cvm.ToList();
        Channels = [..Channels.Concat(channelViewModels).ToList()];
        Logger.Information(@">>>>>>New Channels: {Join}", string.Join(",", Channels.Select(channel => channel.Id)));

        // Update shard index and wallpaper sequence
        foreach (var channel in selectedChannels)
        {
            _shardIndex.Add(channel.Id, 1);
        }

        TrayMenuHelper.AddNewChannelSequence(channelViewModels);
    }

    public void DeleteSelectedChannel(string channelId)
    {
        SavedChannels = string.Join(",", SavedChannels.Split(",").Where(item => item != channelId));

        Properties.Settings.Default.UserUnsplashChannels = SavedChannels;
        Properties.Settings.Default.Save();

        var item = Channels.FirstOrDefault(c => c.Id == channelId);
        if (item != null) Channels.Remove(item);

        // SelectedChannel = Channels[0];
        // SelectedIndex = (sbyte)Channels.IndexOf(SelectedChannel);

        // Refresh shard index and wallpaper sequence
        _shardIndex.Remove(channelId);
        TrayMenuHelper.DelChannelSequence(channelId);
        // Clear cached memory/disk channel photos
        UnsplashCache.UnCacheChannelPhotos(channelId);

        if (channelId != CheckedChannel) return;
        {
            // reset to default
            CheckedChannel = Channels[0].Id;
            Channels.First(c => c.Id == CheckedChannel).IsChecked = true;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}