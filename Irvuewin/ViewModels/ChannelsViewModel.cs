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
    private string SavedChannels { get; set; } = Properties.Settings.Default.UserUnsplashChannels;
    private ObservableCollection<UnsplashPhoto> _photos = [];
    private ObservableCollection<ChannelViewModel> _channels = [];
    private ChannelViewModel _selectedChannel;
    private sbyte _selectedIndex = Properties.Settings.Default.SelectedChannelIndex;
    private int ShardIndex { get; set; } = 1;
    private int PageSize { get; set; } = 12;
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

    public ChannelsViewModel()
    {
        _selectedChannel = new ChannelViewModel(); // useless
        DefaultQuery = new UnsplashQueryParams
        {
            Page = 1,
            PerPage = PageSize,
            Orientation = Properties.Settings.Default.WallpaperOrientation
        };
        ItemSelected = new RelayCommand<ChannelViewModel>(OnListBoxItemSelected);
        LoadMorePhotos = new RelayCommand<object>(OnLoadMorePhotos);
        _ = InitializeAsync();
        Console.WriteLine(@"=========> ChannelsViewModel initialized.");
    }


    private async Task InitializeAsync()
    {
        // Channels
        await LoadChannels();
        Channels[SelectedIndex].IsSelected = true;
        SelectedChannel = Channels[SelectedIndex];
        // Channel's Photos
        await LoadPhotos(SelectedChannel.Id, DefaultQuery);
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
            var httpService = IHttpClient.GetUnsplashHttpService();
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

    private async Task LoadPhotos(string channelId, UnsplashQueryParams query, bool append = false)
        // TODO 自动刷新分页
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
            if (append)
            {
                foreach (var item in cachedPhotos)
                {
                    Photos.Add(item);
                }
            }
            else
            {
                Photos = [..cachedPhotos];
            }
        }
        // load from web
        else
        {
            var httpService = IHttpClient.GetUnsplashHttpService();
            if (await httpService.GetPhotosOfChannel(channelId, query) is { } photos
                && photos.Count != 0)
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
                    Photos = [..photos];
                }

                // update cache
                await CachePhotos(cacheIndex, photos);
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
        IsBusy = true;
        ShardIndex++;
        try
        {
            UnsplashQueryParams query = new()
            {
                Page = ShardIndex,
                PerPage = PageSize,
                Orientation = Properties.Settings.Default.WallpaperOrientation
            };
            await LoadPhotos(SelectedChannel.Id, query, true);
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

    public async void AddChannel(ObservableCollection<UnsplashChannel> selectedChannels)
    {
        // TODO  reduce duplicated
        // 添加到settings，
        // 缓存到本地
        var newChannels = string.Join(",", selectedChannels.Select(channel => channel.Id));
        SavedChannels = SavedChannels + "," + newChannels;
        Console.WriteLine($@">>>>>>SavedChannels: {SavedChannels}");
        
        // Properties.Settings.Default.UserUnsplashChannels = SavedChannels;
        // Properties.Settings.Default.Save();

        // map to ChannelViewModel list
        var cvm = selectedChannels.Select(channel => MapperProvider.Mapper.Map<ChannelViewModel>(channel));
        Channels = [..Channels.Concat(cvm).ToList()];
        Console.WriteLine($@">>>>>>New Channels: {string.Join(",", Channels.Select(channel => channel.Id))}");
        // await CacheChannels();
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