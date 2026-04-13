using Irvuewin.Helpers.DB;
using Irvuewin.Helpers.Utils;

namespace Irvuewin.Tests.Helpers.DB;

[TestClass]
public class DataBaseServiceTests
{
    [TestMethod]
    public void TestGetLikedPhotos()
    {
        var ids = DataBaseService.GetLikedPhotoIds();

        Assert.AreEqual(4, ids.Count);
        foreach (var id in ids)
        {
            Console.WriteLine(@"{0}", Path.Combine(new DirectoryInfo(FileUtils.CachedWallpaperFolder).FullName, id + ".jpg"));
        }
    }
}