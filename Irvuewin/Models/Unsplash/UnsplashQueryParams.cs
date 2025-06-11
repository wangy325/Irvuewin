using System.Text;

namespace Irvuewin.Models.Unsplash;

public class UnsplashQueryParams
{
    private int _page = 1;
    private int _perPage = 10;
    // 0-all 1-landscape 2-portrait 3-squarish
    private byte _orientation = Properties.Settings.Default.WallpaperOrientation;
    
    public int Page
    {
        get => _page;
        set => _page = value;
    }
    
    public int PerPage
    {
        get => _perPage;
        set => _perPage = value;
    }
    
    public byte Orientation
    {
        get => _orientation;
        set => _orientation = value;
    }
    
    public string ToQueryString()
    {
        var query = new StringBuilder("")
            .Append("page=").Append(_page)
            .Append("&per_page=").Append(_perPage);
        switch (_orientation)
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