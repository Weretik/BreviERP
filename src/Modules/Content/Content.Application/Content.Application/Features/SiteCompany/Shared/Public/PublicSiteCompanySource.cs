using Content.Application.Abstractions;
using BuildingBlocks.Application.Caching;

namespace Content.Application.Features.SiteCompany.Shared.Public;

public interface IPublicSiteCompanySource
{
    /// <summary>Null before the facts exist; the site then keeps its own.</summary>
    Task<PublicSiteCompanyResponse?> GetAsync(CancellationToken cancellationToken);
}

/// <summary>The company facts for the public site (CNT-009 R-05), read through <see cref="PublicSiteCache"/>.</summary>
public sealed class PublicSiteCompanySource(IContentDbContext db, IReadCache cache) : IPublicSiteCompanySource
{
    private static readonly ReadCacheEntry Entry = PublicSiteCache.Entry("public-site-company");

    public async Task<PublicSiteCompanyResponse?> GetAsync(CancellationToken cancellationToken)
        => (await cache.GetOrCreateAsync(Entry, ReadAsync, cancellationToken)).Company;

    private async ValueTask<PublicSiteCompanySnapshot> ReadAsync(CancellationToken cancellationToken)
    {
        var info = await db.CompanyInfo.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        return new PublicSiteCompanySnapshot(info is null ? null : SiteCompanyReads.Public(info));
    }
}

/// <summary>The cached answer; null facts are cached too, so a site without them does not read the table each time.</summary>
[System.ComponentModel.ImmutableObject(true)]
public sealed record PublicSiteCompanySnapshot(PublicSiteCompanyResponse? Company);
