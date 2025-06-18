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
    }

    private void AddChannel_Click(object sender, RoutedEventArgs e)
    {
        var selectedItems = PreChannelsListBox.SelectedItems;
        if (selectedItems.Count <= 0)
        {
            MessageBox.Show("Please select at least one channel.");
            return;
        }

        if (DataContext is AddChannelViewModel {} viewModel)
        {
            viewModel.AddChannel();
        }
        Close();
    }

    private void CancelAddChannel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ResolvingChannel_Click(object sender, RoutedEventArgs e)
    {
        // TODO 无输入时禁用按扭
        var url = InputChannelUrl.Text;
        Console.WriteLine($@"ResolvingChannel: {url}");
        
        if (DataContext is AddChannelViewModel {} viewModel)
        {
            // viewModel.ResolvingChannel(url);
        }
    }
}