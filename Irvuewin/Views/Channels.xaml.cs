using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;
using Irvuewin.ViewModels;
using WpfToolkit.Controls;

namespace Irvuewin.Views;

public partial class Channels : LocationAwareWindow
{
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
        Console.WriteLine($@"Window_Loaded: ------------------ ");
        var viewModel = DataContext as ChannelsViewModel;
        var selectedChannel = Properties.Settings.Default.UserSelecctedChannel;
        Console.WriteLine($@"Window_Loaded: Selected channel Index: {selectedChannel}");
        // viewModel!.CheckedChannel = selectedChannel;
    }


    private async void ChannelsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _isInitialized = false;
        // 更新频道数据缓存
        if (DataContext is ChannelsViewModel viewModel)
        {
            Console.WriteLine(@"Window_Closing: Saving channels...");
            // TODO Double check 
            await viewModel.CacheChannels();
        }

        Console.WriteLine(@"Window Closed.");
    }

    private void Channels_Detail_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as ChannelsViewModel;
        var checkedChannel = viewModel!.Channels.First(c => c.Id == viewModel.CheckedChannel);
        Console.WriteLine($@"Selected Channel: {checkedChannel.Title}");
        try
        {
            // Open link with default browser
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = checkedChannel.Links.Html.OriginalString,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"打开链接失败: {ex.Message}");
        }
    }


    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is ChannelsViewModel viewModel)
        {
            _ = viewModel.RefreshPhotos(viewModel.CheckedChannel).ConfigureAwait(false);
        }
    }

    // Scrolling event
    private void VirtualizingItemsControl_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (sender is not VirtualizingItemsControl itemsControl) return;
        var scrollViewer = FindVisualChild<ScrollViewer>(itemsControl);
        // Judge if scroll to bottom
        if (scrollViewer == null) return;
        if (!(scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 10)) return;
        // Load more photos
        if (DataContext is ChannelsViewModel viewModel && viewModel.LoadMorePhotos.CanExecute(null))
        {
            viewModel.LoadMorePhotos.Execute(null);
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
        if (channel.Id == "317099")
        {
            MessageBox.Show("Can not delete system Reserved channel.");
            return;
        }

        if (DataContext is ChannelsViewModel viewModel)
        {
            viewModel.DeleteSelectedChannel(channel.Id);
        }
    }
}