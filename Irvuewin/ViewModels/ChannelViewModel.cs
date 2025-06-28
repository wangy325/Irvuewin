using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;

namespace Irvuewin.ViewModels;


public class ChannelViewModel : UnsplashChannel, INotifyPropertyChanged
{
    private bool _isSelected;
    public bool IsSelected { get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    } 
    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand ItemSelected { set; get; } = new RelayCommand<object>(OnTrayChannelSelected);


    private static void OnTrayChannelSelected(object param)
    {
        if (param is not ChannelViewModel item) return;
        item.IsSelected = true;
        var channelsWindow = ChannelsViewModel.GetInstance();
        foreach (var channel in channelsWindow.Channels!)
        {
            if (channel == item) continue;
            channel.IsSelected = false;
        }
        channelsWindow.SelectedChannel = item;
        // 更新index
        Properties.Settings.Default.SelectedChannelIndex = (sbyte)channelsWindow.Channels.IndexOf(item);
        Properties.Settings.Default.Save();
        Debug.WriteLine(">>>>> selected index saved...");
    }


    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}