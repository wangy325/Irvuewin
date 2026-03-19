using AspectInjector.Broker;
using Irvuewin.Models.Unsplash;
using Serilog;

namespace Irvuewin.Helpers.AOP;

[AttributeUsage(AttributeTargets.Method)]
[Injection(typeof(SizeFilterAspect))]
public class FilterBySizeAttribute : Attribute
{
}

[Aspect(Scope.Global)]
public class SizeFilterAspect
{
    private static readonly ILogger Logger = Log.ForContext<SizeFilterAspect>();

    [Advice(Kind.Before)]
    public void CheckSize([Argument(Source.Arguments)] object[] args)
    {
        var photos = args.OfType<List<UnsplashPhoto>>().FirstOrDefault();
        if (photos == null || photos.Count == 0) return;

        var minRes = Properties.Settings.Default.MinResolution;
        var orientation = Properties.Settings.Default.WallpaperOrientation;
        var display = DisplayInfoHelper.GetDisplayInfo()[0];
        int width;
        int height;

        if (orientation == 0) // 假设 0 为横屏
        {
            // 1. 安全转换浮点型为整型：使用 Convert.ToInt32 (自带四舍五入以及溢出检查，是C#中最安全常用的做法)
            width = Convert.ToInt32(Math.Max(display.Width, display.Height) * minRes);
            height = Convert.ToInt32(Math.Min(display.Width, display.Height) * minRes);
        }
        else
        {
            height = Convert.ToInt32(Math.Max(display.Width, display.Height) * minRes);
            width = Convert.ToInt32(Math.Min(display.Width, display.Height) * minRes);
        }

        // 2. 优雅的LINQ修改方式：
        photos.Where(p => p.Width < width || p.Height < height)
            .ToList()
            .ForEach(p => p.IsTooSmall = true);
        Logger.Information("Size filter...");
    }
}