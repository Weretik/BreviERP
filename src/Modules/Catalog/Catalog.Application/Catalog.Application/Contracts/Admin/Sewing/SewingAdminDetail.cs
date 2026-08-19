namespace Catalog.Application.Contracts.Admin.Sewing;

public sealed record SewingAdminDetail(
    decimal MetersPerProduct,
    IReadOnlyList<FabricAdminDetail> Fabrics,
    IReadOnlyList<AccessoryAdminDetail> Accessories,
    IReadOnlyList<OperationAdminDetail> Operations,
    decimal? PiecesPerShift,
    SewingPrices? Prices);
