namespace Catalog.Application.Contracts.Admin.Sewing;

public sealed record SewingPrices(
    IReadOnlyList<SewingFabricPrices> ByFabric,
    SewingPriceRanges Ranges);

public sealed record SewingFabricPrices(int FabricId, decimal Price1To10, decimal Price11To39, decimal Price40Plus);

public sealed record SewingPriceRanges(
    SewingPriceRange Price1To10,
    SewingPriceRange Price11To39,
    SewingPriceRange Price40Plus);

public sealed record SewingPriceRange(decimal MinPrice, int MinFabricId, decimal MaxPrice, int MaxFabricId);
