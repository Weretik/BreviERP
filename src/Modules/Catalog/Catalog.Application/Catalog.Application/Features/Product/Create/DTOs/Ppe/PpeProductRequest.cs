namespace Catalog.Application.Features.Product.Create.DTOs;

public sealed record PpeProductRequest(
    int SupplierId,
    decimal BasePrice,
    PercentRequest RetailPercent,
    PercentRequest WholesalePercent);
