using BuildingBlocks.Application.Caching;

namespace Content.Application;

/// <summary>
/// The public site reads of this module (catalog, articles, redirects, pages, settings, company, galleries) are the
/// same for every visitor and are cached for <see cref="Lifetime"/> (ADR-0021, spec PLT-005). Every Content command
/// changes something the site shows, so each one drops <see cref="Tag"/>: a save shows on the next request of the
/// same instance. Other instances, and Catalog changes behind the public catalog, show within <see cref="Lifetime"/>.
/// </summary>
public static class PublicSiteCache
{
    public const string Tag = "content:public-site";
    public static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(60);

    public static ReadCacheEntry Entry(string name) => ReadCacheEntry.Create($"content.{name}", Tag, Lifetime);
}
