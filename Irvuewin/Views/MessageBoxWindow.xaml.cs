using System.Windows;
using System.Windows.Media;

namespace Irvuewin.Views
{
    public partial class MessageBoxWindow
    {
        private MessageBoxResult Result { get; set; } = MessageBoxResult.Cancel;

        private MessageBoxWindow()
        {
            InitializeComponent();
        }

        public static MessageBoxResult Show(string messageBoxText, string caption = "Tip", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
        {
            var window = new MessageBoxWindow
            {
                TitleText =
                {
                    Text = caption
                },
                MessageText =
                {
                    Text = messageBoxText
                }
            };
            window.InitButtons(button);
            window.InitIcon(icon);
            
            // Set Owner if possible for centering
            Window? ownerWindow = null;
            if (Application.Current != null)
            {
               foreach (Window w in Application.Current.Windows)
               {
                   if (!w.IsActive || !w.IsVisible) continue;
                   ownerWindow = w;
                   break;
               }

               // Fallback to MainWindow if no active window found (and MainWindow is visible)
               if (ownerWindow == null && Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible && Application.Current.MainWindow.WindowState != WindowState.Minimized)
               {
                   ownerWindow = Application.Current.MainWindow;
               }
            }

            if (ownerWindow != null)
            {
                try 
                {
                    window.Owner = ownerWindow;
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                catch
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            else
            {
                 window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            
            // SystemSounds
            switch(icon)
            {
                case MessageBoxImage.Exclamation: System.Media.SystemSounds.Exclamation.Play(); break;
                case MessageBoxImage.Hand: System.Media.SystemSounds.Hand.Play(); break;
                case MessageBoxImage.Asterisk: System.Media.SystemSounds.Asterisk.Play(); break;
                case MessageBoxImage.Question: System.Media.SystemSounds.Question.Play(); break;
            }

            window.ShowDialog();
            return window.Result;
        }

        private void InitButtons(MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OK:
                    BtnOK.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.OKCancel:
                    BtnOK.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNo:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnNo.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void InitIcon(MessageBoxImage icon)
        {
            var pathData = "";
            var color = "#3585FF";

            switch (icon)
            {
                case MessageBoxImage.Information: // Also Asterisk
                    pathData = "M12,2A10,10 0 1,0 22,12A10,10 0 0,0 12,2M11,17H13V11H11V17M12,9A1,1 0 1,0 11,8A1,1 0 0,0 12,9Z";
                    color = "#3585FF";
                    break;
                case MessageBoxImage.Error: // Also Hand, Stop
                    pathData = "M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z";
                    color = "#F44336";
                    break;
                case MessageBoxImage.Warning: // Also Exclamation
                    pathData = "M12,2L1,21H23M12,6L19.53,19H4.47M11,10V14H13V10M11,16V18H13V16";
                    color = "#FFC107";
                    break;
                case MessageBoxImage.Question:
                    pathData = "M12,2C6.48,2 2,6.48 2,12C2,17.52 6.48,22 12,22C17.52,22 22,17.52 22,12C22,6.48 17.52,2 12,2M13,19H11V17H13M15.07,11.25L14.17,12.17C13.45,12.9 13,13.5 13,15H11V14.5C11,13.4 11.45,12.35 12.17,11.63L13.41,10.38C13.78,10.05 14,9.55 14,9C14,7.9 13.1,7 12,7C10.9,7 10,7.9 10,9H8C8,6.79 9.79,5 12,5C14.21,5 16,6.79 16,9C16,9.88 15.64,10.68 15.07,11.25Z";
                    color = "#3585FF"; 
                    break;
                case MessageBoxImage.None:
                    IconPath.Visibility = Visibility.Collapsed;
                    return;
            }

            try
            {
                IconPath.Data = Geometry.Parse(pathData);
                IconPath.Fill = (Brush)new BrushConverter().ConvertFromString(color)!;
            }
            catch { // ignore
            }
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (Equals(sender, BtnYes)) Result = MessageBoxResult.Yes;
            else if (Equals(sender, BtnNo)) Result = MessageBoxResult.No;
            else if (Equals(sender, BtnOK)) Result = MessageBoxResult.OK;
            else if (Equals(sender, BtnCancel)) Result = MessageBoxResult.Cancel;
            
            Close();
        }
    }
}
