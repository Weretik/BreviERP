namespace Catalog.Application.Features.Product.Queries.GetAdminProductDetail;

public sealed record SewingReadModel(
    decimal MetersPerProduct,
    IReadOnlyList<FabricReadModel> Fabrics,
    IReadOnlyList<AccessoryReadModel> Accessories,
    IReadOnlyList<int> OperationIds);
