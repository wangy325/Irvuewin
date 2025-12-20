using Irvuewin.Helpers;

namespace Irvuewin.Tests.Helpers;

[TestClass]
public class CacheManagerTests
{
    [TestMethod]
    public void TestCacheManager()
    {
        const string key1 = "ping1";
        const string key2 = "ping2";
        const string val = "pong";

        var range = new Dictionary<string, int>()
        {
            ["id"] = 1,
            ["id2"] = 2,
            ["id3"] = 3,
        };

        var rk = range.Select(kvp => (("sequence", kvp.Key), kvp.Value));


        CacheManager.Set(key1, val, expiration: TimeSpan.FromMinutes(1));
        Assert.IsTrue(CacheManager.TryGet(key1, out string? value1));
        Assert.AreEqual(val, value1);

        CacheManager.Set(key1, key2, val);
        Assert.IsTrue(CacheManager.TryGet(key1, key2, out string? value2));
        Assert.AreEqual(val, value2);

        // batch save
        CacheManager.SetRange(rk);
        Assert.IsTrue(CacheManager.TryGet<int>("sequence", "id2", out var value3));
        Assert.AreEqual(2, value3);
        
        // test remove
        CacheManager.Remove<string>(key1, key2);
        var v = CacheManager.Get<string>(key1, key2);
        Assert.IsNull(v);
    }
}