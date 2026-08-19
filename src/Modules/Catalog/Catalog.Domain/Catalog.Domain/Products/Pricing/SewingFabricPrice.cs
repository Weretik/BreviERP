namespace Catalog.Domain.Products.Pricing;

public sealed record SewingFabricPrice(int FabricId, decimal Price1To10, decimal Price11To39, decimal Price40Plus);
