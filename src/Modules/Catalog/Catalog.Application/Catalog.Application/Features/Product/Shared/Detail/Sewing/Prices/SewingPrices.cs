namespace Catalog.Application.Features.Product.Shared.Detail.Sewing.Prices;

public sealed record SewingPrices(
    IReadOnlyList<SewingFabricPrices> ByFabric,
    SewingPriceRanges Ranges);
