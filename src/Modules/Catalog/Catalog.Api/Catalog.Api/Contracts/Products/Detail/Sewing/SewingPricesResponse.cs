namespace Catalog.Api.Contracts.Products;

public sealed record SewingPricesResponse(IReadOnlyList<SewingFabricPricesResponse> ByFabric, SewingPriceRangesResponse Ranges);
