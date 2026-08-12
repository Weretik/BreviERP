namespace Catalog.Application.Contracts.Admin;

public sealed record SewingAdminDetail(
    decimal MetersPerProduct,
    IReadOnlyList<FabricAdminDetail> Fabrics,
    IReadOnlyList<AccessoryAdminDetail> Accessories,
    IReadOnlyList<int> OperationIds);
