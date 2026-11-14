namespace Catalog.Api.Product.Detail.Ppe;

public sealed record PpeResponse(NamedReferenceResponse Supplier, decimal BasePrice, PpePercentResponse RetailPercent, PpePercentResponse WholesalePercent, decimal RetailPrice, decimal WholesalePrice);
