namespace Catalog.Application.Features.Product.Queries.GetAdminProductDetail.Sewing;

public sealed record SewingReadModel(
    decimal MetersPerProduct,
    IReadOnlyList<FabricReadModel> Fabrics,
    IReadOnlyList<AccessoryReadModel> Accessories,
    IReadOnlyList<int> OperationIds);
