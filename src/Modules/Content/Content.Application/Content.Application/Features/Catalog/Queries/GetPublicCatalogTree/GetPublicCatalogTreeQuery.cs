using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogTree;

/// <summary>The catalog tree of the site: types and their sections with products on the site (CNT-002 R-09).</summary>
public sealed record GetPublicCatalogTreeQuery : IQuery<Result<PublicCatalogTree>>;

public sealed class GetPublicCatalogTreeHandler(IPublicCatalogSource source)
    : IQueryHandler<GetPublicCatalogTreeQuery, Result<PublicCatalogTree>>
{
    public static readonly string[] Types = ["Sewing", "Ppe"];

    public async ValueTask<Result<PublicCatalogTree>> Handle(GetPublicCatalogTreeQuery query,
        CancellationToken cancellationToken)
    {
        var snapshot = await source.GetAsync(cancellationToken);
        return new PublicCatalogTree(Types
            .Select(type => new PublicCatalogType(type, snapshot.Products.Count(x => x.Product.Type == type),
                snapshot.Sections.Where(x => x.Type == type)
                    .Select(x => new PublicCatalogSection(x.Id, x.Slug, x.Name, x.ProductCount)).ToArray()))
            .Where(x => x.ProductCount > 0)
            .ToArray());
    }
}
