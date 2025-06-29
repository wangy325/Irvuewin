using static System.Environment;
using static System.Environment.SpecialFolder;

namespace Irvuewin.Helpers;

public interface IAppConst
{
    const int PageSize = 12;
    
    static string DefaultWallpaperDownloadDir = GetFolderPath(MyPictures);

    const string CachedPhotosNamePrefix = "photos_";
    const string CachedPhotosNameSuffix = "cached.json";

    const string ChooseFolder = "选择文件夹";

    struct Hints
    {
        public const string MessageBoxCaption = "提示";
        public const string IllegalFolder = "不能选择系统或隐藏文件夹";
    }
}