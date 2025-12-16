using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Helpers.Utils;
using Irvuewin.Views;


namespace Irvuewin.ViewModels;

public class TrayViewModel : INotifyPropertyChanged
{
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
        set
        {
            _aboutWallpaper = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<WallpaperChangeInterval> _intervals;
    public ObservableCollection<WallpaperChangeInterval> Intervals
    {
        get => _intervals;
        set
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
    }

    private void OnLocalizationChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == Binding.IndexerName || string.IsNullOrEmpty(e.PropertyName))
        {
            // Rebuild intervals to update text
            Intervals = new ObservableCollection<WallpaperChangeInterval>(GenerateIntervals());
            
            OnPropertyChanged(nameof(NextWallpaperChangeTime));
            // Refresh Wallpaper Info text
            AboutWallpaper.RefreshLocalization();
        }
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

    private static string? _lastDisplayName;

    private static async void OnCheckDisplay(object param)
    {
        TrayMenuHelper.CheckPointer();
        var sid = TrayMenuHelper.CurrentScreen.Name;
        
        // Update wallpaper info
        if (_lastDisplayName is not null)
        {
            if (TrayMenuHelper.WallpaperChanged.TryGetValue(sid, out var changed) && changed)
            {
                if (sid != _lastDisplayName)
                {
                    _lastDisplayName = sid;
                    if (Properties.Settings.Default.MultiDisplay != 1) return;
                }

                await TrayMenuHelper.DisplayWallpaperInfo();
                TrayMenuHelper.WallpaperChanged[sid] = false;
            }
            else
            {
                if (sid == _lastDisplayName) return;
                _lastDisplayName = sid;
                if (Properties.Settings.Default.MultiDisplay == 1)
                {
                    await TrayMenuHelper.DisplayWallpaperInfo();
                }
            }
        }
        else
        {
            _lastDisplayName = TrayMenuHelper.CurrentScreen.Name;
            await TrayMenuHelper.DisplayWallpaperInfo();
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
        Task.Run(TrayMenuHelper.LoadCachedSequence);
        Properties.Settings.Default.Save();
    }

    private static void OnSaveCachedSequence(object obj)
    {
        // Save Cache
        TrayMenuHelper.SaveCachedSequence();
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
    public ushort TagInt { get; set; }
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
