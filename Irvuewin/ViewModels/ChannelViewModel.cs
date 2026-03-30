using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;
using LiteDB;

namespace Irvuewin.ViewModels;

public class ChannelViewModel : UnsplashChannel, INotifyPropertyChanged
{
    [BsonIgnore]
    public new string Title
    {
        get => base.Title;
        set
        {
            if (base.Title == value) return;
            base.Title = value;
            OnPropertyChanged();
        }
    }

    private bool _isChecked;

    // 'Checked' means that app will update wallpaper from this channel.
    [BsonIgnore]
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

    [BsonIgnore]
    public bool IsReserved { get; set; }

    [BsonIgnore]
    public ICommand ChannelSelected { set; get; } = new RelayCommand<ChannelViewModel>(OnChannelSelected);

    
    [BsonIgnore]
    public ICommand ChannelChecked { set; get; } = new RelayCommand<ChannelViewModel>(OnChannelChecked);

    private static void OnChannelSelected(ChannelViewModel? param)
    {
        var channelsWindow = ChannelsViewModel.GetInstance();
        channelsWindow.ChannelSelected2.Execute(param);
    }
    

    // Change app status
    private static void OnChannelChecked(ChannelViewModel? param)
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