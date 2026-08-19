namespace Catalog.Api.Contracts.Products;

public sealed record SewingFabricPricesResponse(int FabricId, decimal Price1To10, decimal Price11To39, decimal Price40Plus);
