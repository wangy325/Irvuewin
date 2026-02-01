using AspectInjector.Broker;
using Irvuewin.Models.Unsplash;

namespace Irvuewin.Helpers.AOP;

[AttributeUsage(AttributeTargets.Method)]
[Injection(typeof(BlockListAspect))]
public class FilterByBlockListAttribute : Attribute
{
}

[Aspect(Scope.Global)]
public class BlockListAspect
{
    [Advice(Kind.Before)]
    public void CheckBlockList([Argument(Source.Arguments)] object[] args)
    {
        if (args != null && args.Length > 0 && args[0] is List<UnsplashPhoto> photos)
        {
            // Placeholder: In a real implementation, you would inject a service or read settings
            // var blockedIds = Properties.Settings.Default.BlockedIds; 
            // photos.RemoveAll(p => blockedIds.Contains(p.Id));
            
            // For now, we will just log or leave it empty as the user hasn't provided the exact blocklist source yet.
            // But the infrastructure is here.
        }
    }
}
