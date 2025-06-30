using System.Windows;
using System.Windows.Input;
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
        if (DataContext is not AddChannelViewModel { } viewModel) return;
        var url = InputChannelUrl.Text;
        if (url.Length <= 0) return;
        try
        {
            var tuple = UrlValidator.ValidateUrl(url);
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
        }
        catch (Exception ex)
        {
            // ignore
        }
        finally
        {
            viewModel.IsLoading = false;
        }
    }

    private void InputChannelUrl_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ResolvingChannel_Click(sender, e);
        }
    }

    private void AddChannelWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is not AddChannelViewModel viewModel) return;
        // viewModel.PreChannels.Clear();
        Console.WriteLine(@"Add Channel Window_Closing: clearing channels...");
    }
}