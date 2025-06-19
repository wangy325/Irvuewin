using System.Windows;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;
using Irvuewin.ViewModels;

namespace Irvuewin.Views;

public partial class AddChannel : LocationAwareWindow
{
    public AddChannel()
    {
        InitializeComponent();
        // DataContext = new AddChannelViewModel();
        // Closing += AddChannelWindow_Closing;
    }

    private void AddChannel_Click(object sender, RoutedEventArgs e)
    {
        var selectedItems = PreChannelsListBox.SelectedItems;
        if (selectedItems.Count <= 0)
        {
            MessageBox.Show("Please select at least one channel.");
            return;
        }

        if (DataContext is AddChannelViewModel { } viewModel)
        {
            viewModel.AddChannel();
        }

        Close();
    }

    private void CancelAddChannel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void ResolvingChannel_Click(object sender, RoutedEventArgs e)
    {
        var url = InputChannelUrl.Text;
        if (url.Length <= 0) return;
        Console.WriteLine($@"ResolvingChannel: {url}");
        var tuple = UrlValidator.ValidateUrl(url);
        if (DataContext is not AddChannelViewModel { } viewModel) return;
        viewModel.IsLoading = true;
        if (tuple is ({ } idOrName, var isCollectionId))
        {
            Console.WriteLine($@"ResolvingChannel: {idOrName}, {isCollectionId}");
            var channels = await viewModel.ResolvingChannel(idOrName, isCollectionId);
            Console.WriteLine($@"Pre channel: {string.Join(",", channels.Select(c => c.Id))}");
            // clear textbox
            InputChannelUrl.Text = "";
        }
        else
        {
            // handle input as search keywords
            await viewModel.SearchChannels(url);
        }
        viewModel.IsLoading = false;
        // TODO 如何提示？
        // MessageBox.Show("Invalid URL", "Error");
    }

    private void AddChannelWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is not AddChannelViewModel viewModel) return;
        // viewModel.PreChannels.Clear();
        Console.WriteLine(@"Add Channel Window_Closing: clearing channels...");
    }
}