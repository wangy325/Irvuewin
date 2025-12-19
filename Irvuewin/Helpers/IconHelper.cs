using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;


namespace Irvuewin.Helpers
{
    /// <summary>
    /// System vector icon helper
    /// </summary>
    public static class IconHelper
    {
        public static ImageSource GenerateImageSource(Geometry geometry, Brush brush, double size = 32)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawGeometry(brush, null, geometry);
            }

            var bounds = geometry.Bounds;
            var scale = size / Math.Max(bounds.Width, bounds.Height);
            
            var finalVisual = new DrawingVisual();
            using (var dc = finalVisual.RenderOpen())
            {
                dc.PushTransform(new ScaleTransform(scale, scale));
                dc.PushTransform(new TranslateTransform(-bounds.Left, -bounds.Top));
                dc.DrawGeometry(brush, null, geometry);
                dc.Pop(); 
                dc.Pop(); 
            }

            var rtb = new RenderTargetBitmap(
                (int)size, 
                (int)size, 
                96, 
                96, 
                PixelFormats.Pbgra32);
            
            rtb.Render(finalVisual);
            
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var stream = new MemoryStream();
            encoder.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); 

            return bitmapImage;
        }

        public static Icon GenerateIcon(Geometry geometry, Brush brush, double size = 32)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawGeometry(brush, null, geometry);
            }

            var bounds = geometry.Bounds;
            var scale = size / Math.Max(bounds.Width, bounds.Height);
            
            var finalVisual = new DrawingVisual();
            using (var dc = finalVisual.RenderOpen())
            {
                dc.PushTransform(new ScaleTransform(scale, scale));
                dc.PushTransform(new TranslateTransform(-bounds.Left, -bounds.Top));
                dc.DrawGeometry(brush, null, geometry);
                dc.Pop(); 
                dc.Pop(); 
            }

            var rtb = new RenderTargetBitmap(
                (int)size, 
                (int)size, 
                96, 
                96, 
                PixelFormats.Pbgra32);
            
            rtb.Render(finalVisual);
            
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var stream = new MemoryStream();
            encoder.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            using var bitmap = new Bitmap(stream);
            var hIcon = bitmap.GetHicon();
            try
            {
                // Create a managed Icon from the handle and clone it so we can destroy the handle
                using var tempIcon = Icon.FromHandle(hIcon);
                return (Icon)tempIcon.Clone();
            }
            finally
            {
                NativeMethods.DestroyIcon(hIcon);
            }
        }
    }
}
