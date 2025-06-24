using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Views;

namespace Irvuewin.ViewModels;

public class TrayViewModel : INotifyPropertyChanged
{
    public ICommand TrayMenuOpened { set; get; } = new RelayCommand<object>(CheckDisplayCommand);

    private static void CheckDisplayCommand(object param)
    {
        TrayMenuHelper.CheckPointer();
    }

    public static ObservableCollection<ChannelViewModel> Channels { set; get; } = [];
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
    
    

    public ICommand LoadCachedSequence { get; } = new RelayCommand<object>(OnLoadCachedSequence);
    public ICommand SaveCachedSequence { get; } = new RelayCommand<object>(OnSaveCachedSequence);
    public ICommand OpenAddChannelWindowCommand { get; } = new RelayCommand<object>(OnAddNeeChannel);
    // TODO Constructor initialization
    public ICommand AuthorPageOpenCommand => new RelayCommand<object>(OnAuthorNameClicked);

    private void OnAuthorNameClicked(object obj)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = AboutWallpaper.AuthorProfile,
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
        TrayMenuHelper.LoadCachedSequence();
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
        var handler = PropertyChanged;
        handler?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }
    
}

public class WallpaperInfo : INotifyPropertyChanged
{

    private int _wallpaperLikes;
    public int WallpaperLikes { get => _wallpaperLikes;
        set
        {
            _wallpaperLikes = value;
            OnPropertyChanged();
        }
    }
    
    private int _wallpaperDownloads;
    public int WallpaperDownloads { get=>_wallpaperDownloads;
        set
        {
            _wallpaperDownloads = value;
            OnPropertyChanged();
        }
    }
    
    public string Author { get; set; }
    
    private string _wallpaperAuthor ;

    public string WallpaperAuthor
    {
        get => _wallpaperAuthor;
        set
        {
            _wallpaperAuthor = $"Photo by {value}";
            OnPropertyChanged();
        }
    }
    // public string WallpaperAuthor => $"Photo by {Author}";
    
    // https://unsplash.com/@parentrap/collections
    public string AuthorProfile { get; set; }

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