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

namespace Irvuewin.src.models
{

    // Ensure there is no conflicting partial class declaration for ChannelsWindow in the project.  
    // This class should inherit from LocationAwareWindow only.  
    public partial class ChannelsWindow: LocationAwareWindow
    {
        public ObservableCollection<string> ImageUrls { get; set; } = [];

        public ChannelsWindow()
        {
            InitializeComponent();
            ImageUrls.Add("https://images.unsplash.com/photo-1591123596170-dfcb5090f3de?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3NTIzNjZ8MHwxfHNlYXJjaHwxfHxiZWF1dHklMjBnaXJsfGVufDB8MHx8fDE3NDc5MjYwNDh8MA&ixlib=rb-4.1.0&q=80&w=400");
            ImageUrls.Add("https://images.unsplash.com/photo-1502323777036-f29e3972d82f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3NTIzNjZ8MHwxfHNlYXJjaHwyfHxiZWF1dHklMjBnaXJsfGVufDB8MHx8fDE3NDc5MjYwNDh8MA&ixlib=rb-4.1.0&q=80&w=400");
            ImageUrls.Add("https://images.unsplash.com/photo-1603988089669-c041ac2fe196?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3NTIzNjZ8MHwxfHNlYXJjaHwyfHxiZWF1dHklMjBnaXJsfGVufDB8MHx8fDE3NDc5MjYwNDh8MA&ixlib=rb-4.1.0&q=80&w=400");
            ImageUrls.Add("https://images.unsplash.com/photo-1500917293891-ef795e70e1f6?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3NTIzNjZ8MHwxfHNlYXJjaHwxfHxiZWF1dHklMjBnaXJsfGVufDB8MHx8fDE3NDc5MjYwNDh8MA&ixlib=rb-4.1.0&q=80&w=400");
            this.DataContext = this;
        }
    }
}
