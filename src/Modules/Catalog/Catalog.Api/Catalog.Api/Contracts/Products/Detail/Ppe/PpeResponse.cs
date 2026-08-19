namespace Catalog.Api.Contracts.Products;

public sealed record PpeResponse(NamedReferenceResponse Supplier, decimal BasePrice, PpePercentResponse RetailPercent, PpePercentResponse WholesalePercent, decimal RetailPrice, decimal WholesalePrice);
