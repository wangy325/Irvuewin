using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Irvuewin.Helpers.Utils
{
    public static class WindowManager
    {
        private static readonly Dictionary<string, WeakReference<Window>> Windows = [];

        public static void ShowWindow<T>(string key, Func<T> windowFactory, bool dialog = false) where T : Window
        {
            if (Windows.TryGetValue(key, out var reference) &&
                reference.TryGetTarget(out var window))
            {
                window.Activate();
                return;
            }

            var newWindow = windowFactory();
            newWindow.Closed += (_, _) => Windows.Remove(key);

            Windows[key] = new WeakReference<Window>(newWindow);
            if (!dialog) newWindow.Show();
            else newWindow.ShowDialog();
        }


        public static void SaveWindowPosition<T>(T window, string key) where T : Window
        {
            // if (window == null) return;
            var position = new WindowPosition
            {
                Left = window.Left,
                Top = window.Top,
                Width = window.Width,
                Height = window.Height
            };
            var serializer = new XmlSerializer(typeof(WindowPosition));
            using var stream = new FileStream(FileUtils.WindowPositionPath(key), FileMode.Create);
            serializer.Serialize(stream, position);
        }

        public static void LoadWindowPosition<T>(T window, string key) where T : Window
        {
            // if (window == null) return;
            var config = FileUtils.WindowPositionPath(key);
            if (!File.Exists(config)) return;
            // XmlSerializer 只能处理public类
            var serializer = new XmlSerializer(typeof(WindowPosition));
            using var stream = new FileStream(config, FileMode.Open);
            if (serializer.Deserialize(stream) is WindowPosition position)
            {
                // main screen
                var workArea = SystemParameters.WorkArea;
                // check if window crossline
                var screens = Screen.AllScreens;
                var left = screens.Min(s => s.Bounds.Left);
                var top = screens.Min(s => s.Bounds.Top);
                var inBounds = position.Left >= left && position.Top >= top;
                if (inBounds)
                {
                    window.Left = position.Left;
                    window.Top = position.Top;
                    window.Width = position.Width;
                    window.Height = position.Height;
                }
                else
                {
                    // reset to main screen
                    window.Width = position.Width;
                    window.Height = position.Height;
                    window.Left = (workArea.Width - window.Width) / 2 + workArea.Left;
                    window.Top = (workArea.Height - window.Height) / 2 + workArea.Top;
                }
            }
        }
    }

    public class WindowPosition
    {
        public double Left { get; init; }
        public double Top { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }
    }
}