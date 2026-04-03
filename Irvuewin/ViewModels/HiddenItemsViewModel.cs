using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Helpers.DB;
using Irvuewin.Helpers.Events;
using Irvuewin.Models.Unsplash;
using Serilog;

namespace Irvuewin.ViewModels;

public class HiddenItemsViewModel : INotifyPropertyChanged
{
    private static readonly ILogger Logger = Log.ForContext<HiddenItemsViewModel>();

    public event PropertyChangedEventHandler? PropertyChanged;

    private ObservableCollection<string> _hiddenAuthors = [];

    public ObservableCollection<string> HiddenAuthors
    {
        get => _hiddenAuthors;
        set
        {
            if (_hiddenAuthors == value) return;
            _hiddenAuthors = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasHiddenAuthors));
        }
    }

    private ObservableCollection<UnsplashPhoto> _hiddenPhotos = [];

    public ObservableCollection<UnsplashPhoto> HiddenPhotos
    {
        get => _hiddenPhotos;
        set
        {
            if (_hiddenPhotos == value) return;
            _hiddenPhotos = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasHiddenPhotos));
        }
    }

    public bool HasHiddenAuthors => HiddenAuthors.Count > 0;
    public bool HasHiddenPhotos => HiddenPhotos.Count > 0;

    public ICommand RemoveHiddenAuthorCommand { get; }
    public ICommand RemoveHiddenPhotoCommand { get; }

    public HiddenItemsViewModel()
    {
        RemoveHiddenAuthorCommand = new RelayCommand<string>(OnRemoveHiddenAuthor);
        RemoveHiddenPhotoCommand = new RelayCommand<UnsplashPhoto>(OnRemoveHiddenPhoto);

        LoadHiddenAuthors();
        LoadHiddenPhotos();

        Properties.Settings.Default.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Properties.Settings.Default.UserFilterList))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(LoadHiddenAuthors);
            }
        };

        EventBus.PhotoHidden += () => { System.Windows.Application.Current.Dispatcher.Invoke(LoadHiddenPhotos); };
    }

    private void LoadHiddenAuthors()
    {
        var currentList = Properties.Settings.Default.UserFilterList ?? "";
        var users = currentList.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        HiddenAuthors = new ObservableCollection<string>(users);
    }

    private void LoadHiddenPhotos()
    {
        var photos = DataBaseService.GetHiddenPhotos();
        HiddenPhotos = new ObservableCollection<UnsplashPhoto>(photos);
    }

    // TODO: Think twice, is necessary to refresh photos? 
    private void OnRemoveHiddenAuthor(string username)
    {
        if (!HiddenAuthors.Contains(username)) return;
        HiddenAuthors.Remove(username);
        OnPropertyChanged(nameof(HasHiddenAuthors));

        Properties.Settings.Default.UserFilterList = string.Join(",", HiddenAuthors);
        Properties.Settings.Default.Save();

        DataBaseService.UnblockAuthor(username);
        Logger.Information("Unhidden author: {Username}", username);
    }

    private void OnRemoveHiddenPhoto(UnsplashPhoto photo)
    {
        if (!HiddenPhotos.Contains(photo)) return;
        HiddenPhotos.Remove(photo);
        OnPropertyChanged(nameof(HasHiddenPhotos));

        DataBaseService.UnhidePhoto(photo.Id);
        Logger.Information("Unhidden photo: {PhotoId}", photo.Id);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}