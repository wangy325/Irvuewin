using Irvuewin.Helpers;

namespace Irvuewin.Tests.Helpers;

[TestClass]
public class DisplayInfoHelperTests
{
    [TestMethod]
    public void TestGetDisplayInfo()
    {
        // arrange
        var expectList = new List<Display>
        {
            new()
            {
                name = "Display 1",
                width = 3840,
                height = 2160
            }
        };
        // act
        var displays = DisplayInfoHelper.GetDisplayInfo();

        //  assert
        Assert.AreEqual(expectList.Count, displays.Count);
        for (var i = 0; i < displays.Count; i++)
        {
            var display = displays[i];
            var expect = expectList[i];
            Console.WriteLine($@"{display.name} {display.width}x{display.height}");
            Assert.AreEqual(expect.width, display.width);
            Assert.AreEqual(expect.height, display.height);
        }
    }
}