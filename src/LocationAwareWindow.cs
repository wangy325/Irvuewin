using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Irvue_win.src;

namespace Irvue_win.src
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
