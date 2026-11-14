namespace Catalog.Api.Endpoints.Product.Detail;

public sealed record SewingPricesResponse(IReadOnlyList<SewingFabricPricesResponse> ByFabric, SewingPriceRangesResponse Ranges);
