using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using AutoMapper.Internal;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;

namespace Irvuewin.ViewModels;

public class AddChannelViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly ChannelsViewModel? _channelsViewModel;

    public string InputBox { get; } = "Please input unsplash url";
    public string Resolving { get; } = " + ";
    public string HintTitle { get; } = "You can use URLs from unsplash, such as:";
    public string Hint1 { get; } = "- https://unsplash.com/@leanspok";
    public string Hint2 { get; } = "- https://unsplash.com/collection/276189/colors";


    private ObservableCollection<UnsplashChannel> _preChannels = [];
    private ICommand PreChannelsUpdated { get; }

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
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    public AddChannelViewModel()
    {
        _channelsViewModel = Application.Current.Resources["ChannelsViewModel"] as ChannelsViewModel;
        PreChannelsUpdated = new RelayCommand<object>(OnPreChannelsUpdated);
        Console.WriteLine(@">>>>>>>>>>>> AddChannelViewModel inited..");
    }


    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void AddChannel()
    {
        // Reduce channels already added
        var channels =
            SelectedChannels.Where(
                c => _channelsViewModel!.Channels.All(ch => ch.Id != c.Id)
            ).ToList();
        if (channels.Count <= 0) return;
        _channelsViewModel?.AddChannel(channels);
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
            var channel = await httpService.GetChannelById(tag);
            if (channel is not null)
                res.Add(channel);
        }
        else
        {
            var channels = await httpService.GetUserChannels(tag);
            if (channels is not null)
            {
                res.AddRange(channels);
            }
        }

        // Reduce dul channels
        var newer =
            res.Where(channel => PreChannels.All(c => c.Id != channel.Id))
                .ToList();
        if (newer.Count > 0)
        {
            PreChannels = [..PreChannels.Concat(newer).ToList()];
            // foreach (var channel in newer)
            // {
            //     PreChannels.Add(channel);
            // }
            /*var defaultSelected =
                PreChannels.Where(c => SelectedChannels.All(sc => sc.Id != c.Id)).ToList();*/
            /*foreach (var channel in newer)
            {
                SelectedChannels.Add(channel);
            }*/
            // SelectedChannels = [..SelectedChannels.Concat(defaultSelected).ToList()];
        }

        return [..PreChannels];
    }

    // Update SelectedChannels when PreChannels updated
    private void OnPreChannelsUpdated(object obj)
    {
        var newer =
            PreChannels.Where(c => SelectedChannels.All(sc => sc.Id != c.Id)).ToList();
        SelectedChannels = [..Enumerable.Concat(SelectedChannels, newer).ToList()];
        // SelectedChannels = [..PreChannels];
        /*foreach (var channel in newer)
        {
            SelectedChannels.Add(channel);
        }*/
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
                PreChannels.Add(channel);
            }
        }

        // Get random 10 collections
        // Not all page returns
        // If page too large, api may return empty collection list 
        /*var totalPage = res.TotalPages;
        var random = new Random().Next(1, totalPage);
        query.Page = random;
        if (await httpService.SearchChannels(keywords, query) is not { } res2) return [];
        if (res2.Results is { } results && results.Any(r => true))
        {
            var newChannels =
                results.Where(c => PreChannels.All(r => r.Id != c.Id)).ToList();
            // Do not toggle PreChannelsUpdated command
            // PreChannels = [..PreChannels.Concat(newChannels).ToList()];
            if (newChannels.Count <= 0) return [..PreChannels];
            foreach (var channel in newChannels)
            {
                PreChannels.Add(channel);
            }
        }*/
        return [..PreChannels];
    }
}