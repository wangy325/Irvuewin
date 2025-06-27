using System.IO;

namespace Irvuewin.Helpers.Utils;

public static class FileUtils
{
    public static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "Irvuewin"
    );

    public static readonly string CachedWallpaperFolder = Path.Combine(AppDataFolder, "splash");
    public static readonly string CachedPhotoBaseFolder = Path.Combine(AppDataFolder, "photo");

    static FileUtils()
    {
        Directory.CreateDirectory(CachedWallpaperFolder);
        Directory.CreateDirectory(CachedPhotoBaseFolder);
    }

    // key -> window's tile (unique)  
    public static string WindowPositionPath(string key)
    {
        var fileName = $"{key}.position.xml";
        return Path.Combine(AppDataFolder, fileName);
    }

    // key -> cached file name
    public static string CachePath(string folder, string key)
    {
        var fileName = $"{key}.cached.json";
        return Path.Combine(folder, fileName);
    }

    // Create directory (and subdirectories) under parent directory 
    public static string CreateDir(string parent, params string[] dirNames)
    {
        var pathSegments = new List<string> { parent };
        pathSegments.AddRange(dirNames);

        var dir = Path.Combine(pathSegments.ToArray());
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return dir;
    }

    public static void CopyFileToDir(string source, string dest)
    {
        dest = Path.Combine(dest, Path.GetFileName(source));
        File.Copy(source, dest, overwrite: true);
    }

    // Delete folder 
    public static void DeleteFolder(string dir)
    {
        if (Directory.Exists(dir))
            Directory.Delete(dir, recursive: true);
    }
    
}