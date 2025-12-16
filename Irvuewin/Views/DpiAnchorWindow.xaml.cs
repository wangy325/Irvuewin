using System.Windows;

namespace Irvuewin.Views
{
    public partial class DpiAnchorWindow : Window
    {
        public DpiAnchorWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            var style = Helpers.NativeMethods.GetWindowLong(helper.Handle, Helpers.NativeMethods.GWL_EXSTYLE);
            Helpers.NativeMethods.SetWindowLong(helper.Handle, Helpers.NativeMethods.GWL_EXSTYLE, style | Helpers.NativeMethods.WS_EX_TOOLWINDOW);
        }
    }
}
