
using System.IO;


namespace Irvuewin
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
    }
}
