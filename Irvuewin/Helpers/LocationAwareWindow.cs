using System.ComponentModel;
using System.Windows;
using Irvuewin.Helpers.Utils;

namespace Irvuewin.Helpers
{
    public class LocationAwareWindow: Window
    {
        /// <summary>
        /// Unique identifier for window position persistence.
        /// Defaults to the class name.
        /// </summary>
        private string PersistenceId => GetType().Name;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowManager.LoadWindowPosition(this, PersistenceId);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            WindowManager.SaveWindowPosition(this, PersistenceId);
        }
    }
}
