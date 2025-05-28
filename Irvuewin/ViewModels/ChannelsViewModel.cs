using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Models.Unsplash;


namespace Irvuewin.ViewModels;


public class ChannelsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private ObservableCollection<UPhoto> _photos = [];
    private ObservableCollection<UCollection> _collections = [];
    private UCollection _selectedChannel = null;
    public ICommand ItemSelected { get; }

    private sbyte _selectedIndex = Properties.Settings.Default.SelectedChannelIndex;

    public sbyte SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value) return;
            _selectedIndex = value;
            Properties.Settings.Default.SelectedChannelIndex = _selectedIndex;
            Properties.Settings.Default.Save();
            System.Diagnostics.Debug.WriteLine($"Selected channel Index: {value}");
            OnPropertyChanged();
        }
    }

    public UCollection SelectedChannel
    {
        get => _selectedChannel;
        set
        {
            _selectedChannel = value;
            OnPropertyChanged();
            if (value != null)
            {
                ItemSelected.Execute(value);
            }
        }
    }

    public ObservableCollection<UPhoto> PhotoCollection
    {
        get => _photos;
        set
        {
            _photos = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<UCollection> Collections
    {
        get => _collections;
        set
        {
            _collections = value;
            OnPropertyChanged();
        }
    }


    public ChannelsViewModel()
    {
        var photo = new UPhoto()
        {
            Links = new PhotoLinks()
            {
                Html = "https://unsplash.com/photos/1",
                Download = "https://unsplash.com/photos/1/download",
            },
            Urls = new Urls()
            {
                Small =
                    "https://images.unsplash.com/photo-1500917293891-ef795e70e1f6?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3NTIzNjZ8MHwxfHNlYXJjaHwxfHxiZWF1dHklMjBnaXJsfGVufDB8MHx8fDE3NDc5MjYwNDh8MA&ixlib=rb-4.1.0&q=80&w=200",
            },
            User = new UnsplashUser()
            {
                Id = "1",
                Name = "John Bakator",
                Username = "jxb511",
                ProfileImage = new ProfileImage()
                {
                    Small =
                        "https://images.unsplash.com/profile-fb-1504194982-405c65f1fb61.jpg?ixlib=rb-4.0.3&crop=faces&fit=crop&w=32&h=32",
                },
                Links = new UserLinks()
                {
                    Html = "https://unsplash.com/@jxb511",
                    Photos = "https://unsplash.com/photos/1",
                    Likes = "https://unsplash.com/photos/1",
                    Portfolio = "https://unsplash.com/photos/1",
                }
            }
        };

        var previewphoto = new PreviewPhoto()
        {
            Id = "1",
            Urls = new Urls()
            {
                Small =
                    "https://images.unsplash.com/photo-1500917293891-ef795e70e1f6?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3NTIzNjZ8MHwxfHNlYXJjaHwxfHxiZWF1dHklMjBnaXJsfGVufDB8MHx8fDE3NDc5MjYwNDh8MA&ixlib=rb-4.1.0&q=80&w=200",
            },
        };

        var uCollection = new UCollection()
        {
            Id = "1",
            Title = "Beautiful",
            Description = "Beautiful things",

            Links = new Models.Unsplash.CollectionLinks()
            {
                Html = "https://unsplash.com/collections/raoebyzOILQ/blue",
                Photos = "https://unsplash.com/collections/1/photos",
                Related = "https://unsplash.com/collections/1/related",
            },

            User = new UnsplashUser()
            {
                Id = "1",
                Name = "John Bakator",
                Username = "jxb511",
                ProfileImage = new ProfileImage()
                {
                    Small =
                        "https://images.unsplash.com/profile-fb-1504194982-405c65f1fb61.jpg?ixlib=rb-4.0.3&crop=faces&fit=crop&w=32&h=32",
                },
                Links = new UserLinks()
                {
                    Html = "https://unsplash.com/@jxb511",
                    Photos = "https://unsplash.com/photos/1",
                }
            },
            CoverPhoto = photo,
            PreviewPhotos = [previewphoto, previewphoto, previewphoto, previewphoto, previewphoto, previewphoto]
        };

        var uCollection2 = new UCollection()
        {
            Id = "1",
            Title = "AcanTara Very Long Title",
            Description = "Beautiful things",

            Links = new Models.Unsplash.CollectionLinks()
            {
                Html = "https://google.com",
                Photos = "https://unsplash.com/collections/1/photos",
                Related = "https://unsplash.com/collections/1/related",
            },

            User = new UnsplashUser()
            {
                Id = "1",
                Name = "John Bakator",
                Username = "jxb511",
                ProfileImage = new ProfileImage()
                {
                    Small =
                        "https://images.unsplash.com/profile-fb-1504194982-405c65f1fb61.jpg?ixlib=rb-4.0.3&crop=faces&fit=crop&w=32&h=32",
                },
                Links = new UserLinks()
                {
                    Html = "https://unsplash.com/@jxb511",
                    Photos = "https://unsplash.com/photos/1",
                }
            },
            CoverPhoto = photo,
            PreviewPhotos = [previewphoto, previewphoto, previewphoto, previewphoto, previewphoto, previewphoto]
        };
        var uCollection3 = new UCollection()
        {
            Id = "1",
            Title = "Babyface",
            Description = "Beautiful things",

            Links = new Models.Unsplash.CollectionLinks()
            {
                Html = "https://bing.com",
                Photos = "https://unsplash.com/collections/1/photos",
                Related = "https://unsplash.com/collections/1/related",
            },

            User = new UnsplashUser()
            {
                Id = "1",
                Name = "John Bakator",
                Username = "jxb511",
                ProfileImage = new ProfileImage()
                {
                    Small =
                        "https://images.unsplash.com/profile-fb-1504194982-405c65f1fb61.jpg?ixlib=rb-4.0.3&crop=faces&fit=crop&w=32&h=32",
                },
                Links = new UserLinks()
                {
                    Html = "https://unsplash.com/@jxb511",
                    Photos = "https://unsplash.com/photos/1",
                }
            },
            CoverPhoto = photo,
            PreviewPhotos = [previewphoto, previewphoto, previewphoto, previewphoto, previewphoto, previewphoto]
        };
        var uCollection4 = new UCollection()
        {
            Id = "1",
            Title = "Carplay",
            Description = "Beautiful things",

            Links = new Models.Unsplash.CollectionLinks()
            {
                Html = "https://openai.com",
                Photos = "https://unsplash.com/collections/1/photos",
                Related = "https://unsplash.com/collections/1/related",
            },

            User = new UnsplashUser()
            {
                Id = "1",
                Name = "John Bakator",
                Username = "jxb511",
                ProfileImage = new ProfileImage()
                {
                    Small =
                        "https://images.unsplash.com/profile-fb-1504194982-405c65f1fb61.jpg?ixlib=rb-4.0.3&crop=faces&fit=crop&w=32&h=32",
                },
                Links = new UserLinks()
                {
                    Html = "https://unsplash.com/@jxb511",
                    Photos = "https://unsplash.com/photos/1",
                }
            },
            CoverPhoto = photo,
            PreviewPhotos = [previewphoto, previewphoto, previewphoto, previewphoto, previewphoto, previewphoto]
        };

        _photos = [photo, photo, photo, photo, photo, photo];
        _selectedChannel = uCollection;
        _collections = [uCollection, uCollection2, uCollection3, uCollection4];
        ItemSelected = new RelayCommand<UCollection>(OnItemSelected);
    }

    private void OnItemSelected(Object param)
    {
        if (param is UCollection item)
            _selectedChannel = item;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RelayCommand<T> : ICommand
{
    public event EventHandler? CanExecuteChanged;
    private readonly Action<object>? _execute;
    private readonly Predicate<object>? _canExecute;

    public RelayCommand(Action<object> execute, Predicate<object>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    //public event EventHandler CanExecuteChanged
    //{
    //    add { CommandManager.RequerySuggested += value; }
    //    remove { CommandManager.RequerySuggested -= value; }
    //}
}