using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.SiteCompany.Shared;
using Microsoft.Extensions.Caching.Memory;

namespace Content.Application.PublicSiteCompany;

public interface IPublicSiteCompanySource
{
    /// <summary>Null before the facts exist; the site then keeps its own.</summary>
    Task<PublicSiteCompanyResponse?> GetAsync(CancellationToken cancellationToken);
}

/// <summary>The company facts for the public site (CNT-009 R-05), read at most once a minute.</summary>
public sealed class PublicSiteCompanySource(IContentDbContext db, IMemoryCache cache) : IPublicSiteCompanySource
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(60);
    private const string Key = "content:public-site-company";

    public async Task<PublicSiteCompanyResponse?> GetAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(Key, out PublicSiteCompanyResponse? cached) && cached is not null) return cached;
        var info = await db.CompanyInfo.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (info is null) return null;
        var result = SiteCompanyReads.Public(info);
        cache.Set(Key, result, Lifetime);
        return result;
    }
}
