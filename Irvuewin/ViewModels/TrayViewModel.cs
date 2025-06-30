using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Views;

namespace Irvuewin.ViewModels;

public class TrayViewModel : INotifyPropertyChanged
{
    
    // Static property should implement INotifyPropertyChanged manually
    // Even though it's an observation collection
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
    public static string HAboutCurrentWallpaper { get; } = "About Wallpaper";

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

    public static List<WallpaperChangeInterval> Intervals { get; set; } =
    [
        new(10),
        new(30),
        new(60),
        new(120),
        new(180),
        new(1440),
        new(0)
    ];

    public static string HChangeCurrentWallpaper { get; } = "Change Wallpaper";
    public static string HChangeAllWallpaper { get; } = "Change All Wallpaper";
    public static string HPreviousWallpaper { get; } = "Previous Wallpaper";
    public static string HDownloadWallpaper { get; } = "Download Wallpaper";

    public static int IntervalsItemCount => Intervals.Count;

    public static string HChannelSelector { get; } = "Channels";
    public static string HManageChannel { get; } = "Manage Channel";
    public static string HAddNewChannel { get; } = "+ Add Channel";

    public static string HWallpaperUpdateInterval { get; } = "Update Interval";
    public static string HRandomWallpaper { get; } = "Random Wallpaper";

    public static string HSettings { get; } = "Settings";

    public static string HExit { get; } = "Exit";

    private DateTimeOffset _nextWallpaperChangeTime;

    public DateTimeOffset NextWallpaperChangeTime
    {
        get => _nextWallpaperChangeTime;
        set
        {
            _nextWallpaperChangeTime = value;
            OnPropertyChanged(nameof(NextWallpaperChangeTime));
            Console.WriteLine($@"Next Wallpaper Change Time: {NextWallpaperChangeTime:yyyy-MM-dd HH:mm:ss}");
        }
    }


    // public string NextWallpaperChangeTimeString => NextWallpaperChangeTime.ToString("yyyy-MM-dd HH:mm:ss");


    public ICommand LoadCachedSequenceCommand { get; } = new RelayCommand<object>(OnLoadCachedSequence);
    public ICommand SaveCachedSequenceCommand { get; } = new RelayCommand<object>(OnSaveCachedSequence);

    public ICommand OpenAddChannelWindowCommand { get; } = new RelayCommand<object>(OnAddNeeChannel);

    // TODO Constructor initialization
    public ICommand AuthorInfoPageOpenCommand => new RelayCommand<object>(OnAuthorNameClicked);
    public ICommand WallpaperInfoPageOpenCommand => new RelayCommand<object>(OnWallpaperInfoClicked);

    public ICommand TrayMenuOpenedCommand { get; } = new RelayCommand<object>(OnCheckDisplay);

    private static void OnCheckDisplay(object param)
    {
        TrayMenuHelper.CheckPointer();
    }


    private void OnWallpaperInfoClicked(object obj)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = AboutWallpaper.ProfileLink,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Ignore
        }
    }

    private void OnAuthorNameClicked(object obj)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = AboutWallpaper.AuthorProfilePageLink,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Ignore
        }
    }


    private static void OnAddNeeChannel(object obj)
    {
        new Channels().Show();
        new AddChannel().ShowDialog();
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

public class WallpaperInfo : INotifyPropertyChanged
{
    private string _likes;

    public string Likes
    {
        get => _likes;
        set
        {
            _likes = $"{value} Likes";
            OnPropertyChanged();
        }
    }

    private string _downloads;

    public string Downloads
    {
        get => _downloads;
        set
        {
            _downloads = $"{value} Downloads";
            OnPropertyChanged();
        }
    }

    public static string Profile { get; } = "Wallpaper Details";

    public string ProfileLink { get; set; }


    private string _author;

    public string Author
    {
        get => _author;
        set
        {
            _author = $"Photo by {value}";
            OnPropertyChanged();
        }
    }

    // https://unsplash.com/@parentrap/collections
    public string AuthorProfilePageLink { get; set; }

    private string _location;

    public string Location
    {
        get => _location;
        set
        {
            _location = value;
            OnPropertyChanged();
        }
    }

    // TODO UTC
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

        Header = tag switch
        {
            > 0 and < 60 => $"{TagInt} Minutes",
            60 => $"{TagInt / 60} Hour",
            > 60 and <= 180 => $"{TagInt / 60} Hours",
            1440 => $"{TagInt / 1440} Day",
            _ => "Manual"
        };
    }
}