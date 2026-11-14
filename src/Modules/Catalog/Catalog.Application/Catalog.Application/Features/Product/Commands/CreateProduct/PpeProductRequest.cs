namespace Catalog.Application.Features.Product.Commands.CreateProduct;

public sealed record PpeProductRequest(
    int SupplierId,
    decimal BasePrice,
    PercentRequest RetailPercent,
    PercentRequest WholesalePercent);
