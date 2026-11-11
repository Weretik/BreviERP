using Content.Application.Contracts;

namespace Content.Application.PublicCatalog;

/// <summary>Addresses of every product on the site, for the sitemap and for building the pages ahead of time.</summary>
public sealed record GetPublicCatalogProductSlugsQuery : IQuery<Result<IReadOnlyList<PublicCatalogSlug>>>;

public sealed class GetPublicCatalogProductSlugsQueryHandler(IPublicCatalogSource source)
    : IQueryHandler<GetPublicCatalogProductSlugsQuery, Result<IReadOnlyList<PublicCatalogSlug>>>
{
    public async ValueTask<Result<IReadOnlyList<PublicCatalogSlug>>> Handle(GetPublicCatalogProductSlugsQuery query,
        CancellationToken cancellationToken)
    {
        var snapshot = await source.GetAsync(cancellationToken);
        return snapshot.Products
            .Select(x => new PublicCatalogSlug(x.Page.Slug, x.Page.UpdatedAtUtc, PublicCatalogMapping.HasRussian(x.Page)))
            .ToArray();
    }
}
