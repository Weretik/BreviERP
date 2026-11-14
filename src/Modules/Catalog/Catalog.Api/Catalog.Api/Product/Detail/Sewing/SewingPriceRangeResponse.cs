namespace Catalog.Api.Product.Detail.Sewing;

public sealed record SewingPriceRangeResponse(decimal MinPrice, int MinFabricId, decimal MaxPrice, int MaxFabricId);
