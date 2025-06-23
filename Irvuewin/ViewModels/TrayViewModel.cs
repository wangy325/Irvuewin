using System.Windows.Forms;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Views;

namespace Irvuewin.ViewModels;

public class TrayViewModel
{
    public ICommand TrayMenuOpened { set; get; } = new RelayCommand<object>(CheckDisplayCommand);

    private static void CheckDisplayCommand(object param)
    {
        TrayMenuHelper.CheckPointer(param);
    }

    public string HAboutCurrentWallpaper { get; } = "About Wallpaper";

    public WallpaperInfo WallpaperInfo { get; set; } = new();

    public List<WallpaperChangeInterval> Intervals { get; set; } =
    [
        new(10),
        new(30),
        new(60),
        new(120),
        new(180),
        new(1440),
        new(0)
    ];

    public string HChannelSelector { get; } = "Channels";
    public string HWallpaperUpdateInterval { get; } = "Update Interval";

    public ICommand LoadCachedSequence { get; } = new RelayCommand<object>(OnLoadCachedSequence);
    public ICommand SaveCachedSequence { get; } = new RelayCommand<object>(OnSaveCachedSequence);
    public ICommand OpenAddChannelWindowCommand { get; } = new RelayCommand<object>(OnAddNeeChannel);

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
}

public class WallpaperInfo
{
    public string WallpaperLikes { get; } = "Likes: 900";
    public string WallpaperDownloads { get; } = "Downloads: 261";
    public string WallpaperAuthor { get; } = "Photo by Adam Silver";
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