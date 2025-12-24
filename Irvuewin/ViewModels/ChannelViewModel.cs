using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;

namespace Irvuewin.ViewModels;

public class ChannelViewModel : UnsplashChannel, INotifyPropertyChanged
{
    private bool _isChecked;

    // 'Checked' means that app will update wallpaper from this channel.
    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value) return;
            _isChecked = value;
            OnPropertyChanged();
        }
    }

    public ICommand ChannelSelected { set; get; } = new RelayCommand<ChannelViewModel>(OnChannelSelected);
    public ICommand ChannelChecked { set; get; } = new RelayCommand<ChannelViewModel>(OnChannelChecked);

    private static void OnChannelSelected(ChannelViewModel param)
    {
        var channelsWindow = ChannelsViewModel.GetInstance();
        channelsWindow.ChannelSelected2.Execute(param);
    }
    

    // Change app status
    private static void OnChannelChecked(ChannelViewModel param)
    {
        var channelsWindow = ChannelsViewModel.GetInstance();
        channelsWindow.ChannelChecked2.Execute(param);
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}