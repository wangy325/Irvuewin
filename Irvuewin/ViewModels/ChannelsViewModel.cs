using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Irvuewin.Models.Unsplash;


namespace Irvuewin.ViewModels;

public class ChannelsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private ObservableCollection<UnsplashPhoto> _photos = [];
    private ObservableCollection<UnsplashCollection> _collections = [];
    private UnsplashCollection _selectedChannel = null;
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

    public UnsplashCollection SelectedChannel
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

    public ObservableCollection<UnsplashPhoto> PhotoCollection
    {
        get => _photos;
        set
        {
            _photos = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<UnsplashCollection> Collections
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
        var photo = new UnsplashPhoto()
        {
            Urls = new Urls()
            {
                Small = new Uri(
                    "https://images.unsplash.com/photo-1500917293891-ef795e70e1f6?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3NTIzNjZ8MHwxfHNlYXJjaHwxfHxiZWF1dHklMjBnaXJsfGVufDB8MHx8fDE3NDc5MjYwNDh8MA&ixlib=rb-4.1.0&q=80&w=200"),
            },
            User = new UnsplashUser()
            {
                Id = "1",
                Name = "John Bakator",
                Username = "jxb511",
                ProfileImage = new ProfileImage()
                {
                    Small = new Uri(
                        "https://images.unsplash.com/profile-fb-1504194982-405c65f1fb61.jpg?ixlib=rb-4.0.3&crop=faces&fit=crop&w=32&h=32"),
                },
                Links = new UserLinks()
                {
                    Html = new Uri("https://unsplash.com/@jxb511"),
                   
                }
            }
        };
        
        var UnsplashCollection = new UnsplashCollection()
        {
            Id = "1",
            Title = "Beautiful",
            Description = "Beautiful things",

            Links = new Models.Unsplash.Links()
            {
                Html = new Uri("https://unsplash.com/collections/raoebyzOILQ/blue"),
            },
            CoverPhoto = photo,
        };

        var uCollection2 = new UnsplashCollection()
        {
            Id = "1",
            Title = "AcanTara Very Long Title",
            Description = "Beautiful things",

            Links = new Models.Unsplash.Links()
            {
                Html = new Uri("https://google.com"),
            },

            
            CoverPhoto = photo,
        };
        var uCollection3 = new UnsplashCollection()
        {
            Id = "1",
            Title = "Babyface",
            Description = "Beautiful things",

            Links = new Models.Unsplash.Links()
            {
                Html = new Uri("https://bing.com"),
            },

            CoverPhoto = photo
        };
        var uCollection4 = new UnsplashCollection()
        {
            Id = "1",
            Title = "Carplay",
            Description = "Beautiful things",

            Links = new Models.Unsplash.Links()
            {
                Html = new Uri("https://openai.com"),
               
            },
        
            CoverPhoto = photo
           
        };

        _photos = [photo, photo, photo, photo, photo, photo];
        _selectedChannel = UnsplashCollection;
        _collections = [UnsplashCollection, uCollection2, uCollection3, uCollection4];
        ItemSelected = new RelayCommand<UnsplashCollection>(OnItemSelected);
    }

    private void OnItemSelected(Object param)
    {
        if (param is UnsplashCollection item)
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