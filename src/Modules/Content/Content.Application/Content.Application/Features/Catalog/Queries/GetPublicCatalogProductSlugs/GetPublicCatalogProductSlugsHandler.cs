using Content.Application.Features.Catalog.Shared;
using Content.Application.Features.Catalog.Shared.Links;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogProductSlugs;

public sealed class GetPublicCatalogProductSlugsHandler(IPublicCatalogSource source)
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
