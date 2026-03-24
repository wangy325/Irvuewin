using System.Text;

namespace Irvuewin.Models.Unsplash;

public class UnsplashQueryParams
{
    private UnsplashQueryParams() { }

    public static UnsplashQueryParams Create() => new();

    private int _page = 1;

    private int _perPage = 12;

    public string ToQueryString()
    {
        var query = new StringBuilder("")
            .Append("page=").Append(_page)
            .Append("&per_page=").Append(_perPage);
        
        // 0-landscape 1-portrait 2-both
        switch (Properties.Settings.Default.WallpaperOrientation)
        {
            case 0:
                query.Append("&orientation=landscape");
                break;
            case 1:
                query.Append("&orientation=portrait");
                break;
            case 3: // not supported yet
                query.Append("&orientation=squarish");
                break;
        }
        return query.ToString();
    }

    public UnsplashQueryParams Page(int page)
    {
        _page = page;
        return this;
    }

    public UnsplashQueryParams PerPage(int perPage)
    {
        _perPage = perPage;
        return this;
    }
    
    public int GetPage() => _page;
    public int GetPerPage() => _perPage;
}