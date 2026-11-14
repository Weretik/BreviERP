namespace Catalog.Api.Endpoints.Product.Detail;

public sealed record SewingPriceRangeResponse(decimal MinPrice, int MinFabricId, decimal MaxPrice, int MaxFabricId);
