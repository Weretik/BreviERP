using Catalog.Application.Features.Product.Shared.Detail.Sewing.Prices;

namespace Catalog.Application.Features.Product.Shared.Detail.Sewing;

public sealed record SewingAdminDetail(
    decimal MetersPerProduct,
    IReadOnlyList<FabricAdminDetail> Fabrics,
    IReadOnlyList<AccessoryAdminDetail> Accessories,
    IReadOnlyList<OperationAdminDetail> Operations,
    decimal? PiecesPerShift,
    SewingPrices? Prices);
