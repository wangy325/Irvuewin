using System.Text;

namespace Irvuewin.Models.Unsplash;

public class UnsplashQueryParams
{
    public int Page { get; set; } = 1;

    public int PerPage { get; init; } = 10;

    // 0-landscape 1-portrait 2-both
    public byte? Orientation { get; set; } = Properties.Settings.Default.WallpaperOrientation;

    public string ToQueryString()
    {
        var query = new StringBuilder("")
            .Append("page=").Append(Page)
            .Append("&per_page=").Append(PerPage);
        switch (Orientation)
        {
            case 0:
                query.Append("&orientation=landscape");
                break;
            case 1:
                query.Append("&orientation=portrait");
                break;
            case 3:
                query.Append("&orientation=squarish");
                break;
        }
        return query.ToString();
    }
}