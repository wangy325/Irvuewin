using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Irvuewin.Helpers.Utils
{
    public static class WindowManager
    {
        private static readonly Dictionary<string, WeakReference<Window>> _windows = [];

        public static void ShowWindow<T>(string key, Func<T> windowFactory) where T : Window
        {
            if (_windows.TryGetValue(key, out var reference) &&
                reference.TryGetTarget(out var window))
            {
                window.Activate();
                return;
            }

            var newWindow = windowFactory();
            newWindow.Closed += (s, e) => _windows.Remove(key);

            _windows[key] = new WeakReference<Window>(newWindow);
            newWindow.Show();
        }

        public static void SaveWindowPosition<T>(T window, string key) where T : Window
        {
            if (window == null) return;
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
            if (window == null) return;
            var config = FileUtils.WindowPositionPath(key);
            if (!File.Exists(config)) return;
            // XmlSerializer 只能处理public类
            var serializer = new XmlSerializer(typeof(WindowPosition));
            using var stream = new FileStream(config, FileMode.Open);
            if (serializer.Deserialize(stream) is WindowPosition position)
            {
                // 获取主屏幕的工作区域（扣除任务栏）
                var workArea = SystemParameters.WorkArea;

                // 判断窗口位置是否在有效屏幕区域内
                var isWithinBounds = position.Left >= workArea.Left &&
                                      position.Top >= workArea.Top &&
                                      position.Left + position.Width <= workArea.Right &&
                                      position.Top + position.Height <= workArea.Bottom;

                if (isWithinBounds)
                {
                    window.Left = position.Left;
                    window.Top = position.Top;
                    window.Width = position.Width;
                    window.Height = position.Height;
                }
                else
                {
                    // 超出屏幕范围，则居中显示
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
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}