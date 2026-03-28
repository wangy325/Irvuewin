using System.IO;

namespace Irvuewin.Helpers.Utils;

using static IAppConst;

public static class FileUtils
{
    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? AppName
    );

    public static readonly string CachedWallpaperFolder = Path.Combine(AppDataFolder, WallpaperCacheFolder);
    public static readonly string CachedResourceFolder = Path.Combine(AppDataFolder, ChannelCacheFolder);
    private static readonly string WindowPosFileFolder = Path.Combine(AppDataFolder, WindowPositionFolder);

    static FileUtils()
    {
        Directory.CreateDirectory(CachedWallpaperFolder);
        Directory.CreateDirectory(CachedResourceFolder);
        Directory.CreateDirectory(WindowPosFileFolder);
    }

    /// <summary>
    /// key -> window's tile (unique)  
    /// </summary>
    /// <param name="key"></param>
    /// <returns>window position file</returns>
    public static string WindowPositionPath(string key)
    {
        var fileName = $"{key}.{CachedConfigFileFormat}";
        return Path.Combine(WindowPosFileFolder, fileName);
    }

    // key -> cached file name
    public static string CachePath(string folder, string key)
    {
        var fileName = $"{key}.{CachedResourceFileFormat}";
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