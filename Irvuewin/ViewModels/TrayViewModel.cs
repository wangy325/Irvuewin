using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Helpers.DB;
using Irvuewin.Helpers.Events;
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

    public DateTimeOffset NextWallpaperChangeTime
    {
        get => DateTimeOffset.TryParse(
            Properties.Settings.Default.NextWallpaperChangeTime, out var t)
            ? t
            : DateTimeOffset.MinValue;
        set
        {
            Properties.Settings.Default.NextWallpaperChangeTime =
                value == DateTimeOffset.MinValue ? "" : value.ToString("o");
            Properties.Settings.Default.Save();
            OnPropertyChanged();
        }
    }

    // public ICommand LoadCachedSequenceCommand { get; } = new RelayCommand(OnLoadCachedSequence);
    // public ICommand SaveCachedSequenceCommand { get; } = new RelayCommand(OnSaveCachedSequence);

    public ICommand OpenAddChannelWindowCommand { get; } = new RelayCommand(OnAddNewChannel);

    public ICommand WallpaperInfoPageOpenCommand { get; } = new RelayCommand<string>(ICommonCommands.OpenUrl);

    public ICommand TrayMenuOpenedCommand { get; } = new RelayCommand(OnCheckDisplay);

    public TrayViewModel()
    {
        _intervals = new ObservableCollection<WallpaperChangeInterval>(GenerateIntervals());
        Localization.Instance.PropertyChanged += OnLocalizationChanged;
        EventBus.WallpaperChangedEvent += OnWallpaperChanged;
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

    private static void OnCheckDisplay(object? param)
    {
        IrvuewinCore.CheckPointer();
        var displayName = IrvuewinCore.CurrentPointerDisplay.Name;
        var tray = System.Windows.Application.Current.Resources["TrayViewModel"] as TrayViewModel;

        // Update wallpaper info manually when necessary
        if (_lastDisplayName is not null)
        {
            if (displayName == _lastDisplayName) return;
            _lastDisplayName = displayName;
            // Get from in-memory cache
            tray!.AboutWallpaper =
                tray.LocalWallpaperInfoCache.TryGetValue(displayName, out var wip) ? wip : new WallpaperInfo();
        }
        else
        {
            _lastDisplayName = displayName;
        }
    }

    private void OnWallpaperChanged(object? sender, EventBus.WallpaperChangedEventArgs e)
    {
        // Update info immediately when wallpaper changes
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            UpdateWallpaperInfo(e.DisplayName);
        });
        // UpdateWallpaperInfo(e.DisplayName);
    }

    private void UpdateWallpaperInfo(string displayName)
    {
        if (!FastCacheManager.TryGet(displayName, out string? photoId) || photoId is null) return;

        if (DataBaseService.GetPhotoById(photoId) is not { } photo) return;
        var wpi = new WallpaperInfo
        {
            Likes = photo.Likes.ToString(),
            Downloads = photo.Downloads.ToString(),
            Location = photo.Location?.Name ?? string.Empty,
            ProfileLink = string.Concat(photo.Links.Html.OriginalString, IAppConst.Attribution),
            Author = photo.User.Name,
            AuthorProfilePageLink = string.Concat(photo.User.Links.Html.OriginalString, IAppConst.Attribution)
        };
        AboutWallpaper = wpi;
        // Cache wallpaper info.
        LocalWallpaperInfoCache[displayName] = wpi;
        Logger.Debug("Update tray wallpaper info.");
    }


    private static void OnAddNewChannel(object? obj)
    {
        WindowManager.ShowWindow(nameof(Channels), () => new Channels());
        WindowManager.ShowWindow(nameof(AddChannel), () => new AddChannel(), true);
    }

    /*private static void OnLoadCachedSequence(object? obj)
    {
        // Load Cached resources
        // Task.Run(IrvuewinCore.LoadCachedSequence);
        Properties.Settings.Default.Save();
    }

    private static void OnSaveCachedSequence(object? obj)
    {
        // Save Cache
        // IrvuewinCore.SaveCachedSequence();
        Properties.Settings.Default.Save();
    }*/

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