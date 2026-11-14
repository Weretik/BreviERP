using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogCategorySlugs;

/// <summary>Addresses of every shown category, for the sitemap and for building the pages ahead of time.</summary>
public sealed record GetPublicCatalogCategorySlugsQuery : IQuery<Result<IReadOnlyList<PublicCatalogSlug>>>;

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
