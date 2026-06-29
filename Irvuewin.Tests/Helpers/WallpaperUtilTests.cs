using Irvuewin.Helpers.Utils;

namespace Irvuewin.Tests.Helpers;

[TestClass]
public class WallpaperUtilTests
{

    [TestMethod]
    public void TestGetCurrentWallpapers()
    {
        var wallpapers = WallpaperUtil.GetCurrentWallpapers();
        
        // Assert.AreEqual(2, wallpapers.Length);
        Assert.IsNotNull(wallpapers);

        foreach (var w in wallpapers)
        {
            Console.WriteLine(@"wallpaper: {0}", w);
        }
    }
    
    
}