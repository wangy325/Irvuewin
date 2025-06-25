using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Models;
using Irvuewin.Models.Unsplash;
using Exception = System.Exception;


namespace Irvuewin.ViewModels;

public class ChannelsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string SavedChannels { get; set; } = Properties.Settings.Default.UserUnsplashChannels;
    private ObservableCollection<UnsplashPhoto> _photos = [];
    private ObservableCollection<ChannelViewModel> _channels = [];
    private ChannelViewModel _selectedChannel;

    private sbyte _selectedIndex = Properties.Settings.Default.SelectedChannelIndex;

    // Wallpaper shard index for each channel
    private readonly Dictionary<string, int> _shardIndex = new();
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
    private bool IsBusy { get; set; } = false;
    private UnsplashQueryParams DefaultQuery { get; set; }
    public ICommand ItemSelected { get; set; }
    public ICommand LoadMorePhotos { get; set; }

    public sbyte SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value) return;
            _selectedIndex = value;
            Properties.Settings.Default.SelectedChannelIndex = _selectedIndex;
            Properties.Settings.Default.Save();
            Console.WriteLine($@"Set SelectedIndex==> Selected channel Index: {value}");
            OnPropertyChanged();
        }
    }

    public ChannelViewModel SelectedChannel
    {
        get => _selectedChannel;
        set
        {
            _selectedChannel = value;
            OnPropertyChanged();
            // ItemSelected.Execute(value);
        }
    }

    public ObservableCollection<UnsplashPhoto> Photos
    {
        get => _photos;
        set
        {
            _photos = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ChannelViewModel> Channels
    {
        get => _channels;
        set
        {
            _channels = value;
            OnPropertyChanged();
        }
    }

    /*private ChannelsViewModel()
    {
    }*/

    public static async Task<ChannelsViewModel> GetChannelsViewModelInstance()
    {
        ChannelsViewModel inst = new()
        {
            _selectedChannel = new ChannelViewModel(), // useless
            DefaultQuery = new UnsplashQueryParams
            {
                Page = 1,
                PerPage = IAppConst.PageSize,
                Orientation = Properties.Settings.Default.WallpaperOrientation
            }
        };

        inst.ItemSelected = new RelayCommand<ChannelViewModel>(inst.OnListBoxItemSelected);
        inst.LoadMorePhotos = new RelayCommand<object>(inst.OnLoadMorePhotos);
        await inst.InitializeAsync();
        // Create a copy of Channels in TrayViewModel
        TrayViewModel.Channels = inst.Channels;
        Console.WriteLine(@"=========> ChannelsViewModel initialized.");
        return inst;
    }


    private async Task InitializeAsync()
    {
        // Channels
        await LoadChannels();
        Channels[SelectedIndex].IsSelected = true;
        // Init Channels photo shard index
        foreach (var channel in Channels)
        {
            _shardIndex[channel.Id] = 1;
        }

        SelectedChannel = Channels[SelectedIndex];
        // Load 1st page of channel's photos 
        await LoadPhotos(SelectedChannel.Id, DefaultQuery);
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

            Console.WriteLine($@"Loaded {Channels.Count} channels from web.");
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
        // load from cache
        if (await UnsplashCache.LoadPhotosAsync(cacheIndex) is { } cachedPhotos
            && cachedPhotos.Any())
        {
            RenewChannelPhotos(cachedPhotos, append);
            return true;
        }
        // load from web
        else
        {
            var httpService = IHttpClient.GetUnsplashHttpService();
            if (await httpService.GetPhotosOfChannel(channelId, query) is { } photos
                && photos.Count != 0)
            {
                RenewChannelPhotos(photos, append);
                // update cache
                await CachePhotos(cacheIndex, photos);
                return true;
            }
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

    private async void OnListBoxItemSelected(object param)
    {
        try
        {
            // Reset photos loaded status and shard index
            AllPhotosLoaded = false;
            foreach (var key in _shardIndex.Keys.ToList())
            {
                _shardIndex[key] = 1;
            }

            // Update selected status
            if (param is not ChannelViewModel item) return;
            item.IsSelected = true;
            _selectedChannel = item;
            foreach (var channel in Channels)
            {
                if (channel == _selectedChannel) continue;
                channel.IsSelected = false;
            }

            // Update selected index to settings
            SelectedIndex = (sbyte)Channels.IndexOf(_selectedChannel);
            Properties.Settings.Default.SelectedChannelIndex = SelectedIndex;
            Properties.Settings.Default.Save();
            Console.WriteLine($@"Selected Index saved: {SelectedIndex}");
            // Load photos
            DefaultQuery.Orientation = Properties.Settings.Default.WallpaperOrientation;
            await LoadPhotos(item.Id, DefaultQuery);
        }
        catch (Exception e)
        {
            // TODO handle exception
        }
    }

    private async void OnLoadMorePhotos(object obj)
    {
        if (IsBusy) return;
        if (AllPhotosLoaded) return;
        IsBusy = true;
        _shardIndex[SelectedChannel.Id]++;
        try
        {
            UnsplashQueryParams query = new()
            {
                Page = _shardIndex[SelectedChannel.Id],
                PerPage = IAppConst.PageSize,
                Orientation = Properties.Settings.Default.WallpaperOrientation
            };
            AllPhotosLoaded = !await LoadPhotos(SelectedChannel.Id, query, true);
            IsBusy = false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task RefreshPhotos(string channelId)
    {
        var httpService = IHttpClient.GetUnsplashHttpService();
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

        Console.WriteLine(@$"Refreshed photos for channel {channelId}");
    }

    public async Task CacheChannels()
    {
        await UnsplashCache.CacheChannelsAsync([..Channels]);
    }

    public async Task CachePhotos(PhotosCachePageIndex cachePageIndex, List<UnsplashPhoto> photos)
    {
        await UnsplashCache.CachePhotosAsync(cachePageIndex, photos);
    }

    public async void AddChannel(List<UnsplashChannel> selectedChannels)
    {
        var newChannels = string.Join(",", selectedChannels.Select(channel => channel.Id));
        SavedChannels = SavedChannels + "," + newChannels;
        // System settings
        Properties.Settings.Default.UserUnsplashChannels = SavedChannels;
        Properties.Settings.Default.Save();

        // Map to ChannelViewModel list
        var cvm = selectedChannels.Select(channel => MapperProvider.Mapper.Map<ChannelViewModel>(channel));
        Channels = [..Channels.Concat(cvm).ToList()];
        Console.WriteLine($@">>>>>>New Channels: {string.Join(",", Channels.Select(channel => channel.Id))}");

        // Update shard index and wallpaper sequence
        foreach (var channel in selectedChannels)
        {
            _shardIndex.Add(channel.Id, 1);
        }

        TrayMenuHelper.AddNewChannelSequence(selectedChannels);
    }

    public async void DeleteSelectedChannel()
    {
        Console.WriteLine($@"Saved channels Before: {SavedChannels}");
        var id = SelectedChannel.Id;
        SavedChannels = string.Join(",", SavedChannels.Split(",").Where(item => item != id));
        Console.WriteLine($@"Saved channels After: {SavedChannels}");

        Properties.Settings.Default.UserUnsplashChannels = SavedChannels;
        Properties.Settings.Default.Save();

        var item = Channels.FirstOrDefault(c => c.Id == id);
        if (item != null)
        {
            Channels.Remove(item);
        }

        // Channels = [..Channels.Where(channel => channel.Id != id)];
        // TODO 优化ListBoxItem和Radio的绑定关系
        SelectedChannel = Channels[0];
        SelectedIndex = (sbyte)Channels.IndexOf(SelectedChannel);

        // Refresh shard index and wallpaper sequence
        _shardIndex.Remove(id);
        TrayMenuHelper.DelChannelSequence(id);

        // Clear cached memory/disk channel photos
        UnsplashCache.UncacheChannelPhotos(id);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RelayCommand<T> : ICommand
{
    public event EventHandler? CanExecuteChanged;
    private readonly Action<object>? _execute;
    private readonly Predicate<object>? _canExecute;

    public RelayCommand(Action<object> execute, Predicate<object>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }
}