namespace Catalog.Application.Features.Product.Create.DTOs;

public sealed record SewingProductRequest(
    decimal MetersPerProduct,
    IReadOnlyCollection<FabricRequest> Fabrics,
    IReadOnlyCollection<AccessoryRequest> Accessories,
    IReadOnlyCollection<int> OperationIds);
