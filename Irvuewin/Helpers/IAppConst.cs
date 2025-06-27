using static System.Environment;
using static System.Environment.SpecialFolder;

namespace Irvuewin.Helpers;

public interface IAppConst
{
    const int PageSize = 12;
    
    static string DefaultWallpaperDownloadDir = GetFolderPath(MyPictures);

    const string CachedPhotosNamePrefix = "photos_";
    const string CachedPhotosNameSuffix = "cached.json";

}