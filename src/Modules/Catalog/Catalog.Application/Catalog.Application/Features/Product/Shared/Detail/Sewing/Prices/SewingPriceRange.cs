namespace Catalog.Application.Features.Product.Shared.Detail.Sewing.Prices;

public sealed record SewingPriceRange(decimal MinPrice, int MinFabricId, decimal MaxPrice, int MaxFabricId);
