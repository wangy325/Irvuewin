using System.Collections.Generic;

namespace Irvuewin.Models
{
    public class GitHubRelease
    {
        public string tag_name { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string body { get; set; } = string.Empty;
        public string html_url { get; set; } = string.Empty;
        public List<GitHubAsset> assets { get; set; } = new();
    }

    public class GitHubAsset
    {
        public string name { get; set; } = string.Empty;
        public string browser_download_url { get; set; } = string.Empty;
    }
}
