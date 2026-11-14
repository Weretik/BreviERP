namespace Catalog.Application.Features.Product.Commands.CreateProduct;

public sealed record SewingProductRequest(
    decimal MetersPerProduct,
    IReadOnlyCollection<FabricRequest> Fabrics,
    IReadOnlyCollection<AccessoryRequest> Accessories,
    IReadOnlyCollection<int> OperationIds);
