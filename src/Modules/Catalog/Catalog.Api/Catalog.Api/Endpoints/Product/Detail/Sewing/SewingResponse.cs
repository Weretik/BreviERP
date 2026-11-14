using Catalog.Api.Endpoints.Product.Detail.Sewing.Prices;

namespace Catalog.Api.Endpoints.Product.Detail.Sewing;

public sealed record SewingResponse(decimal MetersPerProduct, IReadOnlyList<FabricResponse> Fabrics, IReadOnlyList<AccessoryResponse> Accessories, IReadOnlyList<OperationResponse> Operations, decimal? PiecesPerShift, SewingPricesResponse? Prices);
