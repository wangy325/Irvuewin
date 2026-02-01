using AspectInjector.Broker;
using Irvuewin.Models.Unsplash;

namespace Irvuewin.Helpers.AOP;

[AttributeUsage(AttributeTargets.Method)]
[Injection(typeof(SizeFilterAspect))]
public class FilterBySizeAttribute : Attribute
{
    public int MinWidth { get; set; } = 0;
    public int MinHeight { get; set; } = 0;
}

[Aspect(Scope.Global)]
public class SizeFilterAspect
{
    [Advice(Kind.Before)]
    public void CheckSize(
        [Argument(Source.Arguments)] object[] args,
        [Argument(Source.Triggers)] Attribute[] triggers
    )
    {
        var attr = triggers.OfType<FilterBySizeAttribute>().FirstOrDefault();
        if (attr == null) return;

        if (args != null && args.Length > 0 && args[0] is List<UnsplashPhoto> photos)
        {
            // Remove photos that do not meet the size requirements
            photos.RemoveAll(p => p.Width < attr.MinWidth || p.Height < attr.MinHeight);
        }
    }
}
