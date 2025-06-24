using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Irvuewin.Helpers
{
    public static class DisplayInfoHelper
    {
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

        public static List<Display> GetDisplayInfo()
        {
            var screens = Screen.AllScreens;
            var displays = new List<Display>();
            foreach (var screen in screens)
            {
                var dm = new DEVMODE();
                dm.dmSize = (short)Marshal.SizeOf(dm);

                if (EnumDisplaySettings(screen.DeviceName, EnumCurrentSettings, ref dm))
                {
                    displays.Add(new Display
                    {
                        name = screen.DeviceName,
                        width = dm.dmPelsWidth,
                        height = dm.dmPelsHeight
                    });
                }
            }

            return displays;
        }


        public static Display CheckDisplay()
        {
            var cursorPosition = Cursor.Position;
            var displays = DisplayInfoHelper.GetDisplayInfo();
            var res = displays[0];
            foreach (var display in displays)
            {
                var displayBounds = new Rectangle(0, 0, display.width, display.height);

                if (!displayBounds.Contains(cursorPosition)) continue;
                // System.Diagnostics.Debug.WriteLine($"鼠标位于显示器: {display.name}，分辨率: {display.width}x{display.height}");
                res = display;
                break;
            }

            return res;
        }
    }

    public struct Display
    {
        public string name;
        public int width;
        public int height;
    }
}