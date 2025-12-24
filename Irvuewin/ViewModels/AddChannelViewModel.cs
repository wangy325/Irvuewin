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
    private static readonly ILogger Logger = Log.ForContext<AddChannelViewModel>();
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly ChannelsViewModel _channelsViewModel;

    public static string Resolving => " + ";
    public static string Hint1 => "- https://unsplash.com/@leanspok";
    public static string Hint2 => "- https://unsplash.com/collection/276189/colors";


    private ICommand SelectedChannelUpdatedCommand { get; }
    
    public ICommand OpenUrlCommand { get; }

    
    
    private ObservableCollection<UnsplashChannel> _searchedChannelResult = [];
    public ObservableCollection<UnsplashChannel> SearchedChannelResult
    {
        get => _searchedChannelResult;
        private set
        {
            _searchedChannelResult = value;
            OnPropertyChanged();
            // Update SelectedChannels to
            // make channels selected by default
            SelectedChannelUpdatedCommand.Execute(null);
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


    private readonly UnsplashHttpService _httpService;

    public AddChannelViewModel()
    {
        _httpService = IHttpClient.GetUnsplashHttpService();
        _channelsViewModel = ChannelsViewModel.GetInstance();
        SelectedChannelUpdatedCommand = new RelayCommand(OnSelectedChannelsUpdated);
        OpenUrlCommand = new RelayCommand(OnUrlOpenButtonClick);
        Logger.Information(@"AddChannelViewModel initialized..");
    }

    private static void OnUrlOpenButtonClick(object? obj)
    {
        ICommonCommands.OpenUrl(IAppConst.UnsplashUrl);
    }


    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
        var res = new List<UnsplashChannel>();
        if (flag)
        {
            if (await ChannelFilter(tag))
            {
                var channel = await _httpService.GetChannelById(tag);
                if (channel != null) res.Add(channel);
            }
            else
            {
                // TODO show hint
            }
        }
        else
        {
            var channels = await _httpService.GetUserChannels(tag);
            if (channels is not null)
            {
                var filteredChannels = await ChannelFilter(channels);
                res.AddRange(filteredChannels);
            }
        }

        // Reduce dul channels
        var newer =
            res.Where(channel => SearchedChannelResult.All(c => c.Id != channel.Id))
                .ToList();
        if (newer.Count > 0)
        {
            SearchedChannelResult = [..SearchedChannelResult.Concat(newer).ToList()];
        }

        return [..SearchedChannelResult];
    }

    // Update SelectedChannels when PreChannels updated
    private void OnSelectedChannelsUpdated(object? obj)
    {
        var newer =
            SearchedChannelResult.Where(c => SelectedChannels.All(sc => sc.Id != c.Id)).ToList();
        SelectedChannels = [..SelectedChannels.Concat(newer).ToList()];
    }

    // Search channels by keywords
    public async Task<List<UnsplashChannel>> SearchChannels(string keywords)
    {
        UnsplashQueryParams query = new()
        {
            Page = 1,
            PerPage = 10,
            Orientation = null
        };
        if (await _httpService.SearchChannels(keywords, query) is not { } res) return [..SearchedChannelResult];
        if (res.Results is { } results && results.Any(_ => true))
        {
            var newChannels =
                results.Where(c => SearchedChannelResult.All(r => r.Id != c.Id)).ToList();
            // Do not toggle PreChannelsUpdated command
            if (newChannels.Count <= 0) return [..SearchedChannelResult];
            foreach (var channel in newChannels)
            {
                if (await ChannelFilter(channel)) SearchedChannelResult.Add(channel);
            }
        }
        return [..SearchedChannelResult];
    }

    // Unavailable channel filter
    // Some channels may contain photos, but can not get by unsplash api
    private async Task<List<UnsplashChannel>> ChannelFilter(List<UnsplashChannel> channels)
    {
        List<UnsplashChannel> res = [];
        foreach (var channel in channels)
        {
            if (await ChannelFilter(channel.Id))
            {
                res.Add(channel);
            }
        }

        return res;
    }

    private async Task<bool> ChannelFilter(UnsplashChannel channel)
    {
        return await ChannelFilter(channel.Id);
    }

    /// <summary>
    /// Channel Filter
    /// </summary>
    /// <param name="channelId">channel id</param>
    /// <returns>Ture if channel contains photos, or else return false.</returns>
    private async Task<bool> ChannelFilter(string channelId)
    {
        return await _httpService.GetPhotosOfChannel(channelId, query: null) is { } res
               && res.Count != 0;
    }
}