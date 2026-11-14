namespace Catalog.Api.Endpoints.Product.Detail.Sewing.Prices;

public sealed record SewingPriceRangeResponse(decimal MinPrice, int MinFabricId, decimal MaxPrice, int MaxFabricId);
