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
        var channelsWindow = ChannelsViewModel.GetInstance();
        channelsWindow.ItemSelected.Execute(item);
    }


    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}