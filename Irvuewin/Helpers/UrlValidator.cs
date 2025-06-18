using System.Text.RegularExpressions;

namespace Irvuewin.Helpers;

public static class UrlValidator
{
        public static (string, bool)? ValidateUrl(string url) 
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
                !(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                return null;
            }

            if (!uriResult.Host.EndsWith("unsplash.com", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            else
            {
                var collectionsRegex =  new Regex(@"^https?://(?:www\.)?unsplash\.com/collections/([^/]+)/([^/]+)$");
                var match = collectionsRegex.Match(url);
                if (match.Success)
                {
                    return (match.Groups[1].Value, true);
                }

                // 判断是否匹配 @username 格式
                var userRegex = new Regex(@"^https?://(?:www\.)?unsplash\.com/@([\w-]+)$");
                match = userRegex.Match(url);
                if (match.Success)
                {
                    return (match.Groups[1].Value, false);
                }
            }
            
            return null;
        }
}