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

    // ,raoebyzOILQ,pY1RsqahQms,62652795
    private string _savedChannels = Properties.Settings.Default.UserUnsplashChannels;
    private ObservableCollection<UnsplashPhoto> _photos = [];
    private ObservableCollection<ChannelViewModel> _channels = [];
    private ChannelViewModel _selectedChannel;
    private sbyte _selectedIndex = Properties.Settings.Default.SelectedChannelIndex;

    private int _shardIndex = 1;
    private const int PageSize = 10;

    private readonly UnsplashQueryParams _defaultQuery = new()
    {
        Page = 1,
        PerPage = PageSize,
        Orientation = Properties.Settings.Default.WallpaperOrientation
    };

    public ICommand ItemSelected { get; set; }

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

    public string SavedChannels
    {
        get => _savedChannels;
        set => _savedChannels = value;
    }

    public ChannelsViewModel()
    {
        _selectedChannel = new ChannelViewModel(); // useless
        _ = InitializeAsync();
        ItemSelected = new RelayCommand<ChannelViewModel>(OnListBoxItemSelected);
        Console.WriteLine(@"=========> ChannelsViewModel initialized.");
    }

    private async Task InitializeAsync()
    {
        // Channels
        await LoadChannels();
        Channels[SelectedIndex].IsSelected = true;
        SelectedChannel = Channels[SelectedIndex];
        // Channel's Photos
        await LoadPhotos(SelectedChannel.Id, _defaultQuery);
    }

    private async Task LoadChannels()
    {
        var channelIds = SavedChannels.Split(',');
        // load from cache
        if (await UnsplashCache.LoadChannelsAsync() is { } cachedChannels
            && cachedChannels.Any()
            && cachedChannels.Count == channelIds.Length)
        {
            List<ChannelViewModel> channelViewModels = [];
            channelViewModels.AddRange(
                cachedChannels.Select(item => MapperProvider.Mapper.Map<ChannelViewModel>(item)));
            Channels = [..channelViewModels];
        }
        // load from web
        else
        {
            var httpService = new UnsplashHttpService(new UnsplashHttpClientWrapper());
            foreach (var id in channelIds)
            {
                if (await httpService.GetChannelById(id) is { } channel)
                    Channels.Add(MapperProvider.Mapper.Map<ChannelViewModel>(channel));
            }

            Console.WriteLine($@"Loaded {Channels.Count} channels from web.");
            // cache channels
            await CacheChannels();
        }
    }

    // TODO 自动刷新分页
    private async Task LoadPhotos(string channelId, UnsplashQueryParams query)
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
            Photos = [..cachedPhotos];
        }
        // load from web
        else
        {
            var httpService = new UnsplashHttpService(new UnsplashHttpClientWrapper());
            if (await httpService.GetPhotosOfChannel(channelId, query) is { } photos
                && photos.Count != 0)
            {
                Photos = [..photos];
                // update cache
                await CachePhotos(cacheIndex);
            }

            Console.WriteLine($@"Loaded photos for channel {channelId} from web api.");
        }
    }

    private async void OnListBoxItemSelected(object param)
    {
        try
        {
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
            _defaultQuery.Orientation = Properties.Settings.Default.WallpaperOrientation;
            await LoadPhotos(item.Id, _defaultQuery);
        }
        catch (Exception e)
        {
            // TODO handle exception
        }
    }

    public async Task RefreshPhotos(string channelId)
    {
        var httpService = new UnsplashHttpService(new UnsplashHttpClientWrapper());
        _defaultQuery.Orientation = Properties.Settings.Default.WallpaperOrientation;
        if (await httpService.GetPhotosOfChannel(channelId, _defaultQuery) is { } photos
            && photos.Count != 0)
        {
            Photos = [..photos];
            // update cache
            var cacheIndex = new PhotosCachePageIndex
            {
                ChannelId = channelId,
                PageIndex = _defaultQuery.Page
            };
            await CachePhotos(cacheIndex);
        }

        Console.WriteLine(@$"Refreshed photos for channel {channelId}");
    }

    public async Task CacheChannels()
    {
        await UnsplashCache.CacheChannelsAsync([..Channels]);
    }

    public async Task CachePhotos(PhotosCachePageIndex cachePageIndex)
    {
        await UnsplashCache.CachePhotosAsync(cachePageIndex, [..Photos]);
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