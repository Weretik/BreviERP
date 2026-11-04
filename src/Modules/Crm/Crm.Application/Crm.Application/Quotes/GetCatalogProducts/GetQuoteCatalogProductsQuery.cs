using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.GetCatalogProducts;

/// <summary>Current Catalog data of chosen products: a saved introduction or a template is shown with it (R-19).</summary>
public sealed record GetQuoteCatalogProductsQuery(IReadOnlyList<int> ProductIds)
    : IQuery<Result<IReadOnlyList<QuoteCatalogProductResponse>>>;
