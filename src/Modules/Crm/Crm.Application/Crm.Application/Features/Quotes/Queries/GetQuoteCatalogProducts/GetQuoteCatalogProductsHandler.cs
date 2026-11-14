using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Abstractions.Quotes;
using Crm.Application.Abstractions.Quotes.Products;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteCatalogProducts;

/// <summary>Products that are no longer in the Catalog are simply missing from the answer.</summary>
public sealed class GetQuoteCatalogProductsHandler(IQuoteCatalogReader catalog)
    : IQueryHandler<GetQuoteCatalogProductsQuery, Result<IReadOnlyList<QuoteCatalogProductResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<QuoteCatalogProductResponse>>> Handle(
        GetQuoteCatalogProductsQuery query, CancellationToken cancellationToken)
    {
        var ids = query.ProductIds.Distinct().ToArray();
        var products = await catalog.GetAsync(ids, cancellationToken);
        return Result.Success<IReadOnlyList<QuoteCatalogProductResponse>>(
            ids.Where(products.ContainsKey).Select(id => products[id].ToResponse()).ToArray());
    }
}
