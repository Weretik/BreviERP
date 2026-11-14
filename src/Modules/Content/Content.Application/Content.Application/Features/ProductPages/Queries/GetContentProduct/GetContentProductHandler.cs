using Content.Application.Abstractions;
using Content.Application.Features.ProductPages.Shared;
using Content.Application.Features.ProductPages.Shared.Responses;

namespace Content.Application.Features.ProductPages.Queries.GetContentProduct;

public sealed class GetContentProductHandler(IContentDbContext db, IContentCatalogReader catalog)
    : IQueryHandler<GetContentProductQuery, Result<ContentProductResponse>>
{
    public async ValueTask<Result<ContentProductResponse>> Handle(GetContentProductQuery query,
        CancellationToken cancellationToken)
    {
        var product = await catalog.GetAsync(query.ProductId, cancellationToken);
        if (product is null) return Result.NotFound(ContentProductMessages.ProductNotFound);
        var page = await db.ProductPages.AsNoTracking().Include(x => x.Texts)
            .FirstOrDefaultAsync(x => x.ProductId == query.ProductId, cancellationToken);
        var offered = page is null ? await ProductPageSlugs.FreeAsync(db, product, cancellationToken) : page.Slug;
        return ContentProductMapping.Detail(product, page, offered,
            await ProductCategoryLinks.IdsAsync(db, product.Id, cancellationToken));
    }
}
