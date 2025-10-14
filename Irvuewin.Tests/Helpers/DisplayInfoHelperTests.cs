using System.Windows.Input;
using Irvuewin.Helpers;
using Irvuewin.Models.Unsplash;

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
                Name = "Display 1",
                Width = 3840,
                LogicWidth = 3840,
                Height = 2160,
                LogicHeight = 2160,
                DsrEnabled = false
            },
            new()
            {
                Name = "Display 2",
                Width = 3840,
                LogicWidth = 2560,
                Height = 2160,
                LogicHeight = 1440,
                DsrEnabled = true
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
            Console.WriteLine($@"{display.Name} {display.Width}x{display.Height}");
            Assert.AreEqual(expect.Width, display.Width);
            Assert.AreEqual(expect.Height, display.Height);
        }
    }

    [TestMethod]
    public void TestCursorPosition()
    {
        DisplayInfoHelper.CheckCursorPosition();
    }
}