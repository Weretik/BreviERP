using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;
using Crm.Application.Abstractions.Quotes.Products;

namespace Crm.Application.Features.Quotes.Queries.GetQuote;

/// <summary>The КП for the editor; items without their own photo show the product's main photo (R-29).</summary>
public sealed class GetQuoteHandler(IQuoteStore store, IQuoteCatalogReader catalog)
    : IQueryHandler<GetQuoteQuery, Result<QuoteResponse>>
{
    public async ValueTask<Result<QuoteResponse>> Handle(GetQuoteQuery query, CancellationToken cancellationToken)
    {
        var quote = await store.Quotes.AsNoTracking()
            .Include(x => x.Items).ThenInclude(x => x.Options).ThenInclude(x => x.Fabrics)
            .Include(x => x.Items).ThenInclude(x => x.Lines)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (quote is null || !query.Actor.CanAccess(quote)) return Result.NotFound(QuoteMessages.NotFound);

        var response = quote.ToResponse();
        var photos = await QuoteItemPhotos.CatalogPhotosAsync(quote, catalog, cancellationToken);
        return photos.Count == 0
            ? Result.Success(response)
            : Result.Success(response with
            {
                Items = response.Items
                    .Select(x => x with { PhotoUrl = QuoteItemPhotos.Resolve(x.ProductId, x.PhotoUrl, photos) })
                    .ToArray()
            });
    }
}
