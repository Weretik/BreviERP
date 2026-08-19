namespace Catalog.Api.Contracts.Products;

public sealed record SewingPriceRangeResponse(decimal MinPrice, int MinFabricId, decimal MaxPrice, int MaxFabricId);
