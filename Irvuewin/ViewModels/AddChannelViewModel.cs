using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;

namespace Irvuewin.ViewModels;
using Serilog;

public class AddChannelViewModel : INotifyPropertyChanged
{
    private static readonly ILogger Logger = Log.ForContext(typeof(AddChannelViewModel));
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly ChannelsViewModel _channelsViewModel;

    public string InputBox { get; } = "Please input unsplash url";
    public string Resolving { get; } = " + ";
    public string HintTitle { get; } = "You can use URLs from unsplash, such as:";
    public string Hint1 { get; } = "- https://unsplash.com/@leanspok";
    public string Hint2 { get; } = "- https://unsplash.com/collection/276189/colors";

    public string Hint4Channels { get; } = " Explore Unsplash to find new channels: ";
    public string ButtonOpenUnsplash { get; } = "Open Unsplash.com";


    private ObservableCollection<UnsplashChannel> _preChannels = [];
    private ICommand PreChannelsUpdated { get; }
    public ICommand OpenUnsplashCommand { get; }

    public ObservableCollection<UnsplashChannel> PreChannels
    {
        get => _preChannels;
        set
        {
            _preChannels = value;
            OnPropertyChanged();
            // Update SelectedChannels to
            // make channels selected by default
            PreChannelsUpdated.Execute(null);
        }
    }

    private ObservableCollection<UnsplashChannel> _selectedChannels = [];

    public ObservableCollection<UnsplashChannel> SelectedChannels
    {
        get => _selectedChannels;
        set
        {
            _selectedChannels = value;
            OnPropertyChanged();
        }
    }

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }


    public AddChannelViewModel()
    {
        _channelsViewModel = ChannelsViewModel.GetInstance();
        PreChannelsUpdated = new RelayCommand<object>(OnPreChannelsUpdated);
        OpenUnsplashCommand = new RelayCommand<object>(OnUnsplashOpenButtonClick);
        Logger.Information(@">>>>>>>>>>>> AddChannelViewModel inited..");
    }

    private void OnUnsplashOpenButtonClick(object obj)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://unsplash.com/collections",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Ignore
        }
    }


    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void AddChannel()
    {
        // Reduce channels already added
        var channels =
            SelectedChannels.Where(c => _channelsViewModel.Channels.All(ch => ch.Id != c.Id)
            ).ToList();
        if (channels.Count <= 0) return;
        _channelsViewModel.AddChannel(channels);
    }

    /// <summary>
    /// Get channel(s) from user/collectionId
    /// </summary>
    /// <param name="tag">username or collectionId</param>
    /// <param name="flag">true-collectionId, false-username</param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<List<UnsplashChannel>> ResolvingChannel(string tag, bool flag)
    {
        var httpService = IHttpClient.GetUnsplashHttpService();
        var res = new List<UnsplashChannel>();
        if (flag)
        {
            if (await ChannelFilter(tag))
            {
                var channel = await httpService.GetChannelById(tag);
                if (channel != null) res.Add(channel);
            }
            else
            {
                // TODO show hint
            }
        }
        else
        {
            var channels = await httpService.GetUserChannels(tag);
            if (channels is not null)
            {
                var filteredChannels = await ChannelFilter(channels);
                res.AddRange(filteredChannels);
            }
        }

        // Reduce dul channels
        var newer =
            res.Where(channel => PreChannels.All(c => c.Id != channel.Id))
                .ToList();
        if (newer.Count > 0)
        {
            PreChannels = [..PreChannels.Concat(newer).ToList()];
        }

        return [..PreChannels];
    }

    // Update SelectedChannels when PreChannels updated
    private void OnPreChannelsUpdated(object obj)
    {
        var newer =
            PreChannels.Where(c => SelectedChannels.All(sc => sc.Id != c.Id)).ToList();
        SelectedChannels = [..Enumerable.Concat(SelectedChannels, newer).ToList()];
    }

    // Search channels by keywords
    public async Task<List<UnsplashChannel>> SearchChannels(string keywords)
    {
        var httpService = IHttpClient.GetUnsplashHttpService();
        UnsplashQueryParams query = new()
        {
            Page = 1,
            PerPage = 10,
            Orientation = null
        };
        if (await httpService.SearchChannels(keywords, query) is not { } res) return [..PreChannels];
        if (res.Results is { } results && results.Any(r => true))
        {
            var newChannels =
                results.Where(c => PreChannels.All(r => r.Id != c.Id)).ToList();
            // Do not toggle PreChannelsUpdated command
            if (newChannels.Count <= 0) return [..PreChannels];
            foreach (var channel in newChannels)
            {
                if (await ChannelFilter(channel)) PreChannels.Add(channel);
            }
        }
        return [..PreChannels];
    }

    // Unavailable channel filter
    // Some channels may contain photos, but can not get by unsplash api
    private async Task<List<UnsplashChannel>> ChannelFilter(List<UnsplashChannel> channels)
    {
        List<UnsplashChannel> res = [];
        var httpClient = IHttpClient.GetUnsplashHttpService();
        foreach (var channel in channels)
        {
            if (await httpClient.GetPhotosOfChannel(channel.Id, query: null) is not { } photo)
            {
                res.Add(channel);
            }
        }

        return res;
    }

    private async Task<bool> ChannelFilter(UnsplashChannel channel)
    {
        var httpClient = IHttpClient.GetUnsplashHttpService();
        return await httpClient.GetPhotosOfChannel(channel.Id, query: null) is { } res
            && res.Any();
    }

    private async Task<bool> ChannelFilter(string channelId)
    {
        var httpClient = IHttpClient.GetUnsplashHttpService();
        return await httpClient.GetPhotosOfChannel(channelId, query: null) is { } res
               && res.Any();
    }
}