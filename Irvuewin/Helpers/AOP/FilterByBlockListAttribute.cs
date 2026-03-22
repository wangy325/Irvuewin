using AspectInjector.Broker;
using Irvuewin.Helpers.DB;
using Irvuewin.Models.Unsplash;
using Serilog;
using Serilog.Core;
using static Irvuewin.Helpers.IAppConst;

namespace Irvuewin.Helpers.AOP;

[AttributeUsage(AttributeTargets.Method)]
[Injection(typeof(BlockListAspect))]
public class FilterByBlockListAttribute : Attribute
{
}

[Aspect(Scope.Global)]
public class BlockListAspect
{
    private static readonly ILogger Logger = Log.ForContext<BlockListAspect>();

    [Advice(Kind.Before)]
    public void CheckBlockList([Argument(Source.Arguments)] object[] args)
    {
        var photos = args.OfType<List<UnsplashPhoto>>().FirstOrDefault();
        if (photos == null || photos.Count == 0) return;
        var blockedUsers =
            Properties.Settings.Default.UserFilterList.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (blockedUsers.Length == 0) return;
        foreach (var photo in photos.Where(photo => blockedUsers.Contains(photo.User.Username)))
            photo.IsBlocked = true;

        // Logger.Information("Blocklist filter...");
    }
}