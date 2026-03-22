using AspectInjector.Broker;
using Irvuewin.Models.Unsplash;
using Serilog;
using static Irvuewin.Helpers.IAppConst;

namespace Irvuewin.Helpers.AOP;

[AttributeUsage(AttributeTargets.Method)]
[Injection(typeof(SmartFilterAspect))]
public class SmartFilterAttribute : Attribute
{
}

[Aspect(Scope.Global)]
public class SmartFilterAspect
{
    private static readonly ILogger Logger = Log.ForContext<SmartFilterAttribute>();

    [Advice(Kind.Before, Targets = Target.Method)]
    public void SmartFilter([Argument(Source.Arguments)] object[] args)
    {
        var photos = args.OfType<List<UnsplashPhoto>>().FirstOrDefault();
        if (photos == null || photos.Count == 0) return;
        var smartFilter = Properties.Settings.Default.SmartFilter;
        if (smartFilter == 0) return;
        foreach (var photo in photos.Where(photo =>
                     PhotoFilterWords.Any(k =>
                         photo.Slug.Contains(k, StringComparison.CurrentCultureIgnoreCase))))
        {
            photo.IsPortrait = true;
        }

        // Logger.Information("Smart filter...");
    }
}