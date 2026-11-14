namespace Catalog.Api.Product.Detail.Sewing;

public sealed record SewingPricesResponse(IReadOnlyList<SewingFabricPricesResponse> ByFabric, SewingPriceRangesResponse Ranges);
