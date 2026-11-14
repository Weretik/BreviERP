namespace Catalog.Api.Endpoints.Product.Detail.Sewing.Prices;

public sealed record SewingPricesResponse(IReadOnlyList<SewingFabricPricesResponse> ByFabric, SewingPriceRangesResponse Ranges);
