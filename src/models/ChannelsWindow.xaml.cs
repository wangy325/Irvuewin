using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Irvuewin.src.unsplash;
using Irvuewin.src.unsplash.photos;

namespace Irvuewin.src.models
{

    // Ensure there is no conflicting partial class declaration for ChannelsWindow in the project.  
    // This class should inherit from LocationAwareWindow only.  
    public partial class ChannelsWindow: LocationAwareWindow
    {
        public ObservableCollection<UnsplashPhoto> Photos { get; set; } = [];

        public ChannelsWindow()
        {
            InitializeComponent();

            var photo = new UnsplashPhoto()
            {
                Links = new Links()
                {
                    Html = "https://unsplash.com/photos/1",
                    Download = "https://unsplash.com/photos/1/download",
                },
                Urls = new Urls()
                {
                    Small = "https://images.unsplash.com/photo-1500917293891-ef795e70e1f6?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3NTIzNjZ8MHwxfHNlYXJjaHwxfHxiZWF1dHklMjBnaXJsfGVufDB8MHx8fDE3NDc5MjYwNDh8MA&ixlib=rb-4.1.0&q=80&w=200",

                },
                User = new UnsplashUser()
                {
                    Id = "1",
                    Name = "John Bakator",
                    Username = "jxb511",
                    ProfileImage = new ProfileImage()
                    {
                        Small = "https://images.unsplash.com/profile-fb-1504194982-405c65f1fb61.jpg?ixlib=rb-4.0.3&crop=faces&fit=crop&w=32&h=32",
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


            Photos.Add(photo);
            Photos.Add(photo);
            Photos.Add(photo);
            Photos.Add(photo);
            Photos.Add(photo);
            this.DataContext = this;
        }
    }
}
