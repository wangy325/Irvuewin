using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;
using Irvuewin.ViewModels;
using WpfToolkit.Controls;
using Serilog;
using Localization = Irvuewin.Helpers.Localization;

namespace Irvuewin.Views;

public partial class Channels
{
    private static readonly ILogger Logger = Log.ForContext<Channels>();
    private bool _isInitialized;

    public Channels()
    {
        InitializeComponent();
        DataContext = ChannelsViewModel.GetInstance();
        Loaded += ChannelsWindow_Loaded;
        Loaded += (_, _) => _isInitialized = true;
        Closing += ChannelsWindow_Closing;
    }


    private void ChannelsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // var viewModel = DataContext as ChannelsViewModel;
        var checkedChannel = Properties.Settings.Default.UserCheckedChannel;
        Logger.Information(@"Window_Loaded: Checked channel Index: {CheckedChannel}", checkedChannel);
        // viewModel!.CheckedChannel = selectedChannel;`
    }


    private async void ChannelsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            _isInitialized = false;
            // 更新频道数据缓存
            if (DataContext is ChannelsViewModel viewModel)
            {
                // TODO Double check 
                await FileCacheManager.CacheChannelsAsync([..viewModel.Channels]);
            }

            Logger.Information(@"Window Closed.");
        }
        catch 
        {
           // ignore
        }
    }

    private void Channels_Detail_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as ChannelsViewModel;
        var checkedChannel = viewModel!.Channels.First(c => c.Id == viewModel.CheckedChannelId);
        ICommonCommands.OpenUrl(checkedChannel.Links.Html.OriginalString);
    }


    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!_isInitialized) return;
        if (ChannelsListBox.SelectedItem is not UnsplashChannel channel) return;
        if (DataContext is ChannelsViewModel viewModel)
        {
            _ = viewModel.RefreshPhotos(channel.Id).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Channel photos preview control scrolling event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void VirtualizingItemsControl_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (sender is not VirtualizingItemsControl itemsControl) return;
        var scrollViewer = FindVisualChild<ScrollViewer>(itemsControl);
        // Judge if scroll to bottom
        if (scrollViewer == null) return;
        if (scrollViewer.ScrollableHeight <= 0) return;
        if (!(scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 10)) return;
        // Load more photos
        if (DataContext is ChannelsViewModel viewModel && viewModel.LoadMorePhotos.CanExecute(null))
        {
            // Use selected channel
            viewModel.LoadMorePhotos.Execute(ChannelsListBox.SelectedItem);
        }
    }

    /// <summary>
    /// Find visual child by type
    /// 查找子控件
    /// </summary>
    /// <param name="parent"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent == null) return null;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
                return result;

            var resultFromChild = FindVisualChild<T>(child);
            if (resultFromChild != null)
                return resultFromChild;
        }

        return null;
    }

    private void AddChannelPage_Click(object sender, RoutedEventArgs e)
    {
        var addChannel = new AddChannel();
        addChannel.ShowDialog();
    }

    private void DeleteChannel_Click(object sender, RoutedEventArgs e)
    {
        if (!_isInitialized) return;
        if (ChannelsListBox.SelectedItem is not UnsplashChannel channel) return;
        // Can not delete system Reserved channel
        if (channel.Id is "317099" or "7282015")
        {
            MessageBoxWindow.Show(Localization.Instance["Channel_Reserved_Channel"], 
                Localization.Instance["Msg_Hint"]);
            return;
        }

        if (DataContext is ChannelsViewModel viewModel)
        {
            viewModel.DeleteSelectedChannel(channel.Id);
        }
    }
}