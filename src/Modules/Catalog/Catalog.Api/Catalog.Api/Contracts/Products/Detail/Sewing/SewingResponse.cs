namespace Catalog.Api.Contracts.Products;

public sealed record SewingResponse(decimal MetersPerProduct, IReadOnlyList<FabricResponse> Fabrics, IReadOnlyList<AccessoryResponse> Accessories, IReadOnlyList<OperationResponse> Operations, decimal? PiecesPerShift, SewingPricesResponse? Prices);
