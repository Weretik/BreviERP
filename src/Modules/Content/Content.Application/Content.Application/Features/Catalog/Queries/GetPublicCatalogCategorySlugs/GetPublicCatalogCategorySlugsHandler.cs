using Content.Application.Features.Catalog.Shared;
using Content.Application.Features.Catalog.Shared.Links;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogCategorySlugs;

public sealed class GetPublicCatalogCategorySlugsHandler(IPublicCatalogSource source)
    : IQueryHandler<GetPublicCatalogCategorySlugsQuery, Result<IReadOnlyList<PublicCatalogSlug>>>
{
    public async ValueTask<Result<IReadOnlyList<PublicCatalogSlug>>> Handle(GetPublicCatalogCategorySlugsQuery query,
        CancellationToken cancellationToken)
    {
        var snapshot = await source.GetAsync(cancellationToken);
        return snapshot.Categories
            .Select(x => new PublicCatalogSlug(x.Category.Slug, x.Category.UpdatedAtUtc,
                PublicCatalogMapping.HasRussian(x.Category)))
            .ToArray();
    }
}
