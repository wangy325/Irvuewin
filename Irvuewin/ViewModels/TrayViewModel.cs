using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Helpers.Utils;
using Irvuewin.Views;
using Serilog;

namespace Irvuewin.ViewModels;

public class TrayViewModel : INotifyPropertyChanged
{
    private static readonly ILogger Logger = Log.ForContext<TrayViewModel>();

    // Static property should implement INotifyPropertyChanged manually
    // Even though it's an observation collection
    private ObservableCollection<ChannelViewModel> _addedChannels = [];

    public ObservableCollection<ChannelViewModel> AddedChannels
    {
        get => _addedChannels;
        set
        {
            _addedChannels = value;
            OnPropertyChanged();
        }
    }

    private WallpaperInfo _aboutWallpaper = new();

    public WallpaperInfo AboutWallpaper
    {
        get => _aboutWallpaper;
        private set
        {
            _aboutWallpaper = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<WallpaperChangeInterval> _intervals;

    public ObservableCollection<WallpaperChangeInterval> Intervals
    {
        get => _intervals;
        private set
        {
            _intervals = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IntervalsItemCount));
        }
    }

    public int IntervalsItemCount => Intervals.Count;

    private DateTimeOffset _nextWallpaperChangeTime;

    public DateTimeOffset NextWallpaperChangeTime
    {
        get => _nextWallpaperChangeTime;
        set
        {
            _nextWallpaperChangeTime = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadCachedSequenceCommand { get; } = new RelayCommand<object>(OnLoadCachedSequence);
    public ICommand SaveCachedSequenceCommand { get; } = new RelayCommand<object>(OnSaveCachedSequence);

    public ICommand OpenAddChannelWindowCommand { get; } = new RelayCommand<object>(OnAddNewChannel);

    public ICommand WallpaperInfoPageOpenCommand { get; } = new RelayCommand<object>(OnWallpaperInfoClicked);

    public ICommand TrayMenuOpenedCommand { get; } = new RelayCommand<object>(OnCheckDisplay);

    public TrayViewModel()
    {
        _intervals = new ObservableCollection<WallpaperChangeInterval>(GenerateIntervals());
        Localization.Instance.PropertyChanged += OnLocalizationChanged;
        IrvuewinCore.WallpaperChangedEvent += OnWallpaperChanged;
    }

    private void OnLocalizationChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != Binding.IndexerName && !string.IsNullOrEmpty(e.PropertyName)) return;
        // Rebuild intervals to update text
        Intervals = new ObservableCollection<WallpaperChangeInterval>(GenerateIntervals());

        OnPropertyChanged(nameof(NextWallpaperChangeTime));
        // Refresh Wallpaper Info text
        AboutWallpaper.RefreshLocalization();
    }

    private static List<WallpaperChangeInterval> GenerateIntervals()
    {
        return
        [
            new WallpaperChangeInterval(10),
            new WallpaperChangeInterval(30),
            new WallpaperChangeInterval(60),
            new WallpaperChangeInterval(120),
            new WallpaperChangeInterval(180),
            new WallpaperChangeInterval(1440),
            new WallpaperChangeInterval(0)
        ];
    }

    private Dictionary<string, WallpaperInfo> LocalWallpaperInfoCache { get; } = new();

    private static string? _lastDisplayName;
    
    private static void OnCheckDisplay(object param)
    {
        IrvuewinCore.CheckPointer();
        var sid = IrvuewinCore.CurrentPointerDisplay.Name;
        var tray = System.Windows.Application.Current.Resources["TrayViewModel"] as TrayViewModel;

        // Update wallpaper info manually when necessary
        if (_lastDisplayName is not null)
        {
            if (sid == _lastDisplayName) return;
            _lastDisplayName = sid;
            // Get from in-memory cache
            tray!.AboutWallpaper =
                tray.LocalWallpaperInfoCache.TryGetValue(sid, out var wip) ? wip : new WallpaperInfo();
        }
        else
        {
            _lastDisplayName = sid;
        }
    }

    private void OnWallpaperChanged(object? sender, IrvuewinCore.WallpaperChangedEventArgs e)
    {
        // Update info immediately when wallpaper changes
        System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            await UpdateWallpaperInfo(e.DisplayName);
        });
    }

    private async Task UpdateWallpaperInfo(string displayName)
    {
        Logger.Debug("Update tray wallpaper info.");
        if (!CacheManager.TryGet(displayName, out string? photoId) || photoId is null) return;

        var httpService = IHttpClient.GetUnsplashHttpService();
        if (await httpService.GetPhotoInfoById(photoId) is { } photo)
        {
            var wpi = new WallpaperInfo
            {
                Likes = photo.Likes.ToString(),
                Downloads = photo.Downloads.ToString(),
                Location = photo.Location.Name,
                ProfileLink = photo.Links.Html.OriginalString,
                Author = photo.User.Name,
                AuthorProfilePageLink = photo.User.Links.Html.OriginalString
            };
            AboutWallpaper = wpi;
            // Cache wallpaper info.
            LocalWallpaperInfoCache[displayName] = wpi;
        }
    }

    private static void OnWallpaperInfoClicked(object obj)
    {
        try
        {
            var url = obj as string;
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    private static void OnAddNewChannel(object obj)
    {
        WindowManager.ShowWindow(nameof(Channels), () => new Channels());
        WindowManager.ShowWindow(nameof(AddChannel), () => new AddChannel(), true);
    }

    private static void OnLoadCachedSequence(object obj)
    {
        // Load Cached resources
        Task.Run(IrvuewinCore.LoadCachedSequence);
        Properties.Settings.Default.Save();
    }

    private static void OnSaveCachedSequence(object obj)
    {
        // Save Cache
        IrvuewinCore.SaveCachedSequence();
        Properties.Settings.Default.Save();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class WallpaperInfo : INotifyPropertyChanged
{
    private string _likesValue = "";

    public string Likes
    {
        get => string.Format(Localization.Instance["Wallpaper_Likes"], _likesValue);
        set
        {
            _likesValue = value;
            OnPropertyChanged();
        }
    }

    private string _downloadsValue = "";

    public string Downloads
    {
        get => string.Format(Localization.Instance["Wallpaper_Downloads"], _downloadsValue);
        set
        {
            _downloadsValue = value;
            OnPropertyChanged();
        }
    }

    // Profile property isn't bound to data, just static resource "Wallpaper Details"
    // But it was a static property in previous code: public static string Profile => ...
    // Now make it instance and dynamic
    public string Profile => Localization.Instance["Wallpaper_Details"];

    private string _profileLink = "";

    public string ProfileLink
    {
        get => _profileLink;
        set
        {
            _profileLink = value;
            OnPropertyChanged();
        }
    }

    private string _authorValue = "";

    public string Author
    {
        get => string.Format(Localization.Instance["Wallpaper_PhotoBy"], _authorValue);
        set
        {
            _authorValue = value;
            OnPropertyChanged();
        }
    }

    // https://unsplash.com/@parentrap/collections
    private string _authorProfilePageLink = "";

    public string AuthorProfilePageLink
    {
        get => _authorProfilePageLink;
        set
        {
            _authorProfilePageLink = value;
            OnPropertyChanged();
        }
    }

    private string _location = "";

    public string Location
    {
        get => _location;
        set
        {
            _location = value;
            OnPropertyChanged();
        }
    }

    public void RefreshLocalization()
    {
        OnPropertyChanged(nameof(Likes));
        OnPropertyChanged(nameof(Downloads));
        OnPropertyChanged(nameof(Author));
        OnPropertyChanged(nameof(Profile));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class WallpaperChangeInterval
{
    private ushort TagInt { get; set; }
    public string Tag { get; set; }
    public string Header { get; set; }

    public WallpaperChangeInterval(ushort tag)
    {
        TagInt = tag;
        Tag = tag.ToString();

        // Use Localization.Instance to get strings, ensuring current culture is used
        var resources = Localization.Instance;
        Header = tag switch
        {
            > 0 and < 60 => string.Format(resources["Interval_Minutes"], TagInt),
            60 => string.Format(resources["Interval_Hour"], TagInt / 60),
            > 60 and <= 180 => string.Format(resources["Interval_Hours"], TagInt / 60),
            1440 => string.Format(resources["Interval_Day"], TagInt / 1440),
            _ => resources["Interval_Manual"]
        };
    }
}