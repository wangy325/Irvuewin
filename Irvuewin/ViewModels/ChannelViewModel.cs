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

    public ICommand ItemChecked { set; get; } = new RelayCommand<object>(OnChannelChecked);



    // Change app status
    private static void OnChannelChecked(object param)
    {
        if (param is not ChannelViewModel item) return;
        var channelsWindow = ChannelsViewModel.GetInstance();
        channelsWindow.ItemSelected.Execute(item);
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}