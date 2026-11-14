namespace Catalog.Application.Features.Product.Shared;

public sealed record SewingPriceRange(decimal MinPrice, int MinFabricId, decimal MaxPrice, int MaxFabricId);
