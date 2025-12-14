using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Irvuewin.Helpers
{
    using Serilog;
    public static class DisplayInfoHelper
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(DisplayInfoHelper));
        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;

            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;

            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevmode);

        private const int EnumCurrentSettings = -1;

        private static List<Display> _displays = [];

        public static List<Display> GetDisplayInfo()
        {
            _displays.Clear();
            var screens = Screen.AllScreens;
            foreach (var screen in screens)
            {
                var dm = new DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(dm);

                if (!EnumDisplaySettings(screen.DeviceName, EnumCurrentSettings, ref dm)) continue;

                // var index = _displays.FindIndex(d => d.Name == screen.DeviceName);
                _displays.Add(new Display
                {
                    Name = screen.DeviceName,
                    Width = dm.dmPelsWidth,
                    LogicWidth = screen.Bounds.Width,
                    Height = dm.dmPelsHeight,
                    LogicHeight = screen.Bounds.Height,
                    X = screen.Bounds.X,
                    Y = screen.Bounds.Y,
                    Left = screen.Bounds.Left,
                    Right = screen.Bounds.Right,
                    Top = screen.Bounds.Top,
                    Bottom = screen.Bounds.Bottom
                });
            }

            // _displays.ForEach(d => d.DsrEnabled = IsDsrEnabled(d));

            return _displays;
        }


        private static bool IsDsrEnabled(Display display)
        {
            // 计算分辨率比例（DSR通常使用1.25x, 1.5x, 2.0x等比例）
            var widthRatio = (double)display.Width / display.LogicWidth;
            var heightRatio = (double)display.Height / display.LogicHeight;

            // 检查比例是否接近DSR常用比例（允许一定误差）
            var isDsrEnabled = IsCloseToDsrRatio(widthRatio) && IsCloseToDsrRatio(heightRatio);

            Logger.Debug($@"DSR检测: {display.Name}, 物理分辨率: {display.Width}x{display.Height}, " +
                              $"逻辑分辨率: {display.LogicWidth}x{display.LogicHeight}, " +
                              $"比例: {widthRatio:F2}x{heightRatio:F2}, " +
                              $"结果: {(isDsrEnabled ? "已启用" : "未启用")}");

            return isDsrEnabled;
        }

        private static bool IsCloseToDsrRatio(double ratio)
        {
            // DSR常见比例：1.25, 1.5, 2.0（允许±0.05的误差）
            double[] dsrRatios = [1.25, 1.5, 1.78, 2.0, 2.2, 3.0, 4.0];
            const double tolerance = 0.05;

            foreach (var r in dsrRatios)
            {
                if (Math.Abs(ratio - r) <= tolerance)
                    return true;
            }

            return false;
        }


        public static Display CheckCursorPosition()
        {
            var cursorPosition = Cursor.Position;
            Logger.Debug(@"Cursor position: {CursorPositionX}, {CursorPositionY}", cursorPosition.X, cursorPosition.Y);

            var displays = DisplayInfoHelper.GetDisplayInfo();
            var res = displays[0];
            foreach (var d in from d in displays
                     let displayBounds = new Rectangle(d.X, d.Y, d.LogicWidth, d.LogicHeight)
                     where displayBounds.Contains(cursorPosition)
                     select d)
            {
                Logger.Debug(@"Display info: {DName}, {DWidth}x{DHeight}", d.Name, d.Width, d.Height);
                res = d;
                break;
            }

            return res;
        }
    }

    public struct Display
    {
        public string Name;
        public int Width;
        public int LogicWidth;
        public int Height;

        public int LogicHeight;

        // not used
        public bool DsrEnabled;

        public int X;
        public int Y;
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }
}