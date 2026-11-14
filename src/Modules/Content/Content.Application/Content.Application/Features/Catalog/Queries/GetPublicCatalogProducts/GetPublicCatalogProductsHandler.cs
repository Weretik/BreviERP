using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogTree;
using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogProducts;

public sealed class GetPublicCatalogProductsHandler(IPublicCatalogSource source)
    : IQueryHandler<GetPublicCatalogProductsQuery, Result<PublicCatalogCardPage>>
{
    public async ValueTask<Result<PublicCatalogCardPage>> Handle(GetPublicCatalogProductsQuery query,
        CancellationToken cancellationToken)
    {
        var snapshot = await source.GetAsync(cancellationToken);
        var listed = snapshot.Products;
        if (query.Category is not null)
        {
            var category = snapshot.Categories.FirstOrDefault(x => x.Category.Slug == query.Category);
            if (category is null) return Result.NotFound("Розділ не знайдено.");
            listed = category.Products;
        }

        var found = listed
            .Where(x => query.Type is null || x.Product.Type == query.Type)
            .Where(x => query.SectionId is null || x.Product.SectionId == query.SectionId)
            .ToArray();
        var items = found.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => PublicCatalogMapping.Card(x, query.Language)).ToArray();
        return new PublicCatalogCardPage(items, query.Page, query.PageSize, found.Length);
    }
}
