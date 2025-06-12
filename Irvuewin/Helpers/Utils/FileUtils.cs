using System.IO;

namespace Irvuewin.Helpers.Utils
{
    public static class FileUtils
    {
        public static readonly string AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "Irvuewin"
        );

        static FileUtils()
        {
            Directory.CreateDirectory(AppDataFolder);
        }

        // key -> window's tile (unique)  
        public static string WindowPositionPath(string key)
        {
            var fileName = $"{key}.position.xml";
            return Path.Combine(AppDataFolder, fileName);
        }

        // key -> cached file name
        public static string CachePath(string key)
        {
            var fileName = $"{key}.cached.json";
            return Path.Combine(AppDataFolder, fileName);
        }

        public static string CreateDir(string parent, string dirName)
        {
            var dir = Path.Combine(parent, dirName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        public static void CopyFileToDir(string source, string dest)
        {
            dest = Path.Combine(dest, Path.GetFileName(source));
            File.Copy(source, dest, overwrite:  true);
        }
    }
}