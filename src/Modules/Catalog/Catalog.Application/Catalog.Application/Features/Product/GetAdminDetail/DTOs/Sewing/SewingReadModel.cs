namespace Catalog.Application.Features.Product.GetAdminDetail.DTOs;

public sealed record SewingReadModel(
    decimal MetersPerProduct,
    IReadOnlyList<FabricReadModel> Fabrics,
    IReadOnlyList<AccessoryReadModel> Accessories,
    IReadOnlyList<int> OperationIds);
