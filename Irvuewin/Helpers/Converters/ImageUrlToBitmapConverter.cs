using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Irvuewin.Helpers.Converters
{
    public class ImageUrlToBitmapConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var url = value as string;
            if (value is Uri uri) url = uri.ToString();

            if (string.IsNullOrWhiteSpace(url)) return null;
            
            // Use cloudflare proxy to avoid 443 errors
            url = url.Replace(IAppConst.OriginImageUrl, IAppConst.ImageProxyUrl);
            var decodeWidth = 0;
            if (parameter != null)
            {
                int.TryParse(parameter.ToString(), out decodeWidth);
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
                if (decodeWidth > 0)
                {
                    bitmap.DecodePixelWidth = decodeWidth;
                }
                // Only keep IgnoreColorProfile to avoid overhead. 
                // DelayCreation is removed as it causes blank images in some WPF virtualization scenarios.
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmap.EndInit();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
