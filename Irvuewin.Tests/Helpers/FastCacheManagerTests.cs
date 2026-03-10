using Irvuewin.Helpers;
using Irvuewin.Helpers.DB;

namespace Irvuewin.Tests.Helpers;

[TestClass]
public class FastCacheManagerTests
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


        FastCacheManager.Set(key1, val, expiration: TimeSpan.FromSeconds(10));
        Assert.IsTrue(FastCacheManager.TryGet(key1, out string? value1));
        Assert.AreEqual(val, value1);
        
        Thread.Sleep(TimeSpan.FromSeconds(10));
        Assert.IsFalse(FastCacheManager.TryGet<string>(key1, out _));

        FastCacheManager.Set(key1, key2, val);
        Assert.IsTrue(FastCacheManager.TryGet(key1, key2, out string? value2));
        Assert.AreEqual(val, value2);

        // batch save
        FastCacheManager.SetRange(rk);
        Assert.IsTrue(FastCacheManager.TryGet<int>("sequence", "id2", out var value3));
        Assert.AreEqual(2, value3);
        
        // test remove
        FastCacheManager.Remove<string>(key1, key2);
        var v = FastCacheManager.Get<string>(key1, key2);
        Assert.IsNull(v);
        
        // test exists
        Assert.IsFalse(FastCacheManager.Exists<string>(key1));
        Assert.IsTrue(FastCacheManager.Exists<int>("sequence", "id3"));
    }
}