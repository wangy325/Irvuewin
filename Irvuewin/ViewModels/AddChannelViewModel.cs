using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Irvuewin.Models.Unsplash;

namespace Irvuewin.ViewModels
{
    public class AddChannelViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly ChannelsViewModel? _channelsViewModel;

        public string InputBox { get; } = "Please input unsplash url";
        public string Resolving { get; } = " + ";
        public string HintTitle { get; } = "You can use URLs from unsplash, such as:";
        public string Hint1 { get; } = "- https://unsplash.com/@leanspok";
        public string Hint2 { get; } = "- https://unsplash.com/collection/276189/colors";
        
        
        

        public List<UnsplashChannel>? PreChannels { get; set; } =
        [
            new UnsplashChannel
            {
                Id = "raoebyzOILQ",
                Title = "Blue",
                ShareKey = "1b2525b4bfce502c199213d2e738273d",
                UpdatedAt = new DateTime(2025, 05, 22, 18, 38, 06, 00, DateTimeKind.Utc),
                CoverPhoto = new UnsplashPhoto
                {
                    Urls = new Urls
                    {
                        Small = new Uri(
                            "https://images.unsplash.com/photo-1519821767025-2b43a48282ca?ixlib=rb-4.1.0&q=80&fm=jpg&crop=entropy&cs=tinysrgb&w=400&fit=max"),
                    }
                },
                TotalPhotos = 123,
            },
            new UnsplashChannel
            {
                Id = "a1b2c3d4e5f6",
                Title = "High Quality Wallpaper of MacOS",
                ShareKey = "abcdef1234567890",
                UpdatedAt = new DateTime(2025, 05, 22, 18, 38, 06, 00, DateTimeKind.Utc),
                CoverPhoto = new UnsplashPhoto
                {
                    Urls = new Urls
                    {
                        Small = new Uri(
                            "https://images.unsplash.com/photo-1742156345582-b857d994c84e?ixlib=rb-4.1.0&q=80&fm=jpg&crop=entropy&cs=tinysrgb&w=400&fit=max"),
                    }
                },
                TotalPhotos = 456,
            }
        ];


        public ObservableCollection<UnsplashChannel> SelectedChannels { get; set; } = [];

        public AddChannelViewModel()
        {
            _channelsViewModel = Application.Current.Resources["ChannelsViewModel"] as ChannelsViewModel;
            if (PreChannels is not null)
                SelectedChannels = new ObservableCollection<UnsplashChannel>(PreChannels);
            System.Diagnostics.Debug.WriteLine(">>>>>>>>>>>> AddChannelViewModel inited..");
        }

        public AddChannelViewModel(ChannelsViewModel? channelsViewModel)
        {
            _channelsViewModel = channelsViewModel;
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddChannel()
        {
            _channelsViewModel?.AddChannel(SelectedChannels);
        }
    }
}