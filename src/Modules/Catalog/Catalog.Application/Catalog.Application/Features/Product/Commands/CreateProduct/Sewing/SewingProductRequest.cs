namespace Catalog.Application.Features.Product.Commands.CreateProduct.Sewing;

public sealed record SewingProductRequest(
    decimal MetersPerProduct,
    IReadOnlyCollection<FabricRequest> Fabrics,
    IReadOnlyCollection<AccessoryRequest> Accessories,
    IReadOnlyCollection<int> OperationIds);
