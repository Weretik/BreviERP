using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.ProductPages.Shared;

namespace Content.Application.ProductPages.GetOne;

/// <summary>The editor of one product; a product without a page comes with empty texts and an offered slug.</summary>
public sealed record GetContentProductQuery(int ProductId) : IQuery<Result<ContentProductResponse>>;

public sealed class GetContentProductQueryValidator : AbstractValidator<GetContentProductQuery>
{
    public GetContentProductQueryValidator() => RuleFor(x => x.ProductId).GreaterThan(0);
}

public sealed class GetContentProductQueryHandler(IContentDbContext db, IContentCatalogReader catalog)
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
        return ContentProductMapping.Detail(product, page, offered);
    }
}
