using System.Windows;
using Irvuewin.Helpers;
using Irvuewin.Models;
using Irvuewin.ViewModels;

namespace Irvuewin.Views
{
    public partial class UpdateWindow : LocationAwareWindow
    {
        public UpdateWindow(GitHubRelease release)
        {
            InitializeComponent();
            DataContext = new UpdateWindowViewModel(release, this);
        }
    }
}
