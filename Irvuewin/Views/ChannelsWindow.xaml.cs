using System.Windows;
using System.Windows.Controls;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;
using Irvuewin.ViewModels;

namespace Irvuewin.Views;

public partial class ChannelsWindow : LocationAwareWindow
{
    private bool _isInitialized = false;

    public ChannelsWindow()
    {
        InitializeComponent();
        this.Loaded += ChannelsWindow_Loaded;
        this.Loaded += (s, e) => _isInitialized = true;
        this.Closing += ChannelsWindow_Closing;
    }


    /// <summary>
    /// 频道页
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Channel_Page_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as ChannelsViewModel;

        if (viewModel?.SelectedChannel is UCollection sc)
        {
            System.Diagnostics.Debug.WriteLine($"Selected Channel: {sc.Title}");
            if (sc.Links.Html is string url)
            {
                System.Diagnostics.Debug.WriteLine($"Opening URL: {url}");
                try
                {
                    // 使用默认浏览器打开链接
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
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

    private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;

        if (sender is ListBox listBox && DataContext is ChannelsViewModel viewModel)
        {
            var selectedItem = listBox.SelectedItem;
            System.Diagnostics.Debug.WriteLine($"ListBox_SelectionChanged: {selectedItem}");
            viewModel.ItemSelected.Execute(selectedItem);
            // 更新index
            Properties.Settings.Default.SelectedChannelIndex = (sbyte)listBox.SelectedIndex;
            Properties.Settings.Default.Save();
            System.Diagnostics.Debug.WriteLine($"ListBox_Selection_Index saved: {listBox.SelectedIndex}");
        }
    }

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

    private void ChannelsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Window Closing...");
        _isInitialized = false;
    }
}