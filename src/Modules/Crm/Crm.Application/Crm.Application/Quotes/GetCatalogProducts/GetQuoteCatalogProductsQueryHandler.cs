using Crm.Application.Quotes.Catalog;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Mapping;

namespace Crm.Application.Quotes.GetCatalogProducts;

/// <summary>Products that are no longer in the Catalog are simply missing from the answer.</summary>
public sealed class GetQuoteCatalogProductsQueryHandler(IQuoteCatalogReader catalog)
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
