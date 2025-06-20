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
    private bool _isInitialized = false;

    public Channels()
    {
        InitializeComponent();
        Loaded += ChannelsWindow_Loaded;
        Loaded += (s, e) => _isInitialized = true;
        Closing += ChannelsWindow_Closing;
    }

    /// <summary>
    /// Load channel data when window is loaded
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ChannelsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Window_Loaded: ------------------ ");
        if (DataContext is ChannelsViewModel viewModel)
        {
            var selectedChannelIndex = Properties.Settings.Default.SelectedChannelIndex;
            System.Diagnostics.Debug.WriteLine($"Window_Loaded: Selected channel Index: {selectedChannelIndex}");
            if (selectedChannelIndex > 0)
                viewModel.SelectedIndex = selectedChannelIndex;
        }
    }

    /// <summary>
    /// Serializing channel data when window is closing
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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

    /// <summary>
    /// Channel's list selection changed event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;

        if (sender is ListBox listBox && DataContext is ChannelsViewModel viewModel)
        {
            var selectedItem = listBox.SelectedItem;
            System.Diagnostics.Debug.WriteLine($"ListBox_SelectionChanged Index: {listBox.SelectedIndex}");
            // 传入索引
            if (listBox.SelectedIndex != Properties.Settings.Default.SelectedChannelIndex)
            {
                viewModel.SelectedIndex = (sbyte)listBox.SelectedIndex;
                viewModel.ItemSelected.Execute(selectedItem);
            }
        }
    }

    /// <summary>
    /// channel's details info page url
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Channels_Detail_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as ChannelsViewModel;

        if (viewModel?.SelectedChannel is UnsplashChannel sc)
        {
            System.Diagnostics.Debug.WriteLine($"Selected Channel: {sc.Title}");
            if (sc.Links.Html is { } url)
            {
                System.Diagnostics.Debug.WriteLine($"Opening URL: {url}");
                try
                {
                    // 使用默认浏览器打开链接
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url.OriginalString,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"打开链接失败: {ex.Message}");
                }
            }
        }
    }


    /// <summary>
    /// Refresh photos of channel
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private async void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is ChannelsViewModel viewModel)
        {
            await viewModel.RefreshPhotos(viewModel.SelectedChannel.Id);
        }
    }

    /// <summary>
    /// Scroll update
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
        if (ChannelsListBox.SelectedItem is not UnsplashChannel channel) return;
        // Can not delete system Reserved channel
        if (channel.Id == "317099")
        {
            MessageBox.Show("Can not delete system Reserved channel.");
            return;
        }
        if (DataContext is ChannelsViewModel viewModel)
        {
            
            viewModel.DeleteSelectedChannel();
        }
    }
}