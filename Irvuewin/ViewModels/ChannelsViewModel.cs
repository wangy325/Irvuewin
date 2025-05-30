using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Models;
using Irvuewin.Models.Unsplash;


namespace Irvuewin.ViewModels;

public class ChannelsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // ,raoebyzOILQ,pY1RsqahQms,62652795
    private string _savedChannels = Properties.Settings.Default.UserUnsplashChannels;
    private ObservableCollection<UnsplashPhoto> _photos = [];
    private ObservableCollection<UnsplashChannel> _channels = [];
    private UnsplashChannel _selectedChannel = null;
    private sbyte _selectedIndex = Properties.Settings.Default.SelectedChannelIndex;


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
            Debug.WriteLine($"Selected channel Index: {value}");
            OnPropertyChanged();
        }
    }

    public UnsplashChannel SelectedChannel
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

    public ObservableCollection<UnsplashChannel> Channels
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
        Channels = [];
        Photos = [];
        SelectedChannel = new UnsplashChannel();
        ItemSelected = new RelayCommand<UnsplashChannel>(OnItemSelected);
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await LoadChannels();
        SelectedChannel = Channels[SelectedIndex];

        // channel Photos
        var query = new UnsplashQueryParams
        {
            Page = 1,
            PerPage = 10,
            Orientation = Properties.Settings.Default.WallpaperOrientation
        };
        await LoadPhotos(SelectedChannel.Id, query);
    }

    private async Task LoadChannels()
    {
        var channelIds = SavedChannels.Split(',');
        // load from cache
        // var cachedChannels = await UnsplashCache.LoadChannelsAsync();
        if (await UnsplashCache.LoadChannelsAsync() is { } cachedChannels
            && cachedChannels.Any()
            && cachedChannels.Count == channelIds.Length)
        {
            Channels = [..cachedChannels];
        }
        else
        {
            // load from web api
            var httpService = new UnsplashHttpService(new UnsplashHttpClientWrapper());
            foreach (var id in channelIds)
            {
                if (await httpService.GetChannelById(id) is { } channel)
                    Channels.Add(channel);
            }

            Debug.WriteLine($"Loaded {Channels.Count} channels from web api.");
            // cache channels
            await UnsplashCache.SaveChannelsASync([..Channels]);
        }
    }

    // 自动分页刷新
    private async Task LoadPhotos(string channelId, UnsplashQueryParams query)
    {
        // load from cache
        var cacheIndex = new PhotosCachePageIndex
        {
            ChannelId = channelId,
            PageIndex = query.Page
        };
        if (await UnsplashCache.LoadPhotosAsync(cacheIndex) is { } cachedPhotos
            && cachedPhotos.Any())
        {
            Photos = [..cachedPhotos];
        }
        else
        {
            // load from web api
            var httpService = new UnsplashHttpService(new UnsplashHttpClientWrapper());

            if (await httpService.GetPhotosOfChannel(channelId, query) is { } photos
                && photos.Any())
            {
                Photos = [..photos];
                // update cache
                await UnsplashCache.SavePhotosAsync(cacheIndex, [..Photos]);
            }

            Debug.WriteLine($"Loaded photos for channel {channelId} from web api.");
        }
    }

    private async void OnItemSelected(Object param)
    {
        if (param is UnsplashChannel item)
        {
            _selectedChannel = item;

            var query = new UnsplashQueryParams
            {
                Page = 1,
                PerPage = 10,
                Orientation = Properties.Settings.Default.WallpaperOrientation
            };

            await LoadPhotos(item.Id, query);
        }
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