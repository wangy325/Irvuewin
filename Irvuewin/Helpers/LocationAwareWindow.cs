using System.ComponentModel;
using System.Windows;
using Irvuewin.Helpers.Utils;

namespace Irvuewin.Helpers
{
    public class LocationAwareWindow: Window
    {
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowManager.LoadWindowPosition(this, this.Title);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            WindowManager.SaveWindowPosition(this, this.Title);
        }
    }
}
