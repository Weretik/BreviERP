namespace Catalog.Api.Endpoints.Product.Detail.Sewing.Prices;

public sealed record SewingFabricPricesResponse(int FabricId, decimal Price1To10, decimal Price11To39, decimal Price40Plus);
