using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irvuewin.src
{
    public static class FileUtils
    {
        public static readonly String AppDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "Irvue-win"
        );

        static FileUtils()
        {
            Directory.CreateDirectory(AppDataFolder);
        }

        // key -> window's tile (unique)  
        public static String WindowPositionPath(String key)
        {
            string fileName = $"{key}.position.xml";

            return Path.Combine(AppDataFolder, fileName);
        }
    }
}
