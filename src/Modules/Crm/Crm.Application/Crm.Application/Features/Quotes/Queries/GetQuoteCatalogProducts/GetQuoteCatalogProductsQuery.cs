using Crm.Application.Features.Quotes.Shared;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteCatalogProducts;

/// <summary>Current Catalog data of chosen products: a saved introduction or a template is shown with it (R-19).</summary>
public sealed record GetQuoteCatalogProductsQuery(IReadOnlyList<int> ProductIds)
    : IQuery<Result<IReadOnlyList<QuoteCatalogProductResponse>>>;
