namespace Catalog.Application.Features.Product.Shared;

public sealed record SewingPrices(
    IReadOnlyList<SewingFabricPrices> ByFabric,
    SewingPriceRanges Ranges);
