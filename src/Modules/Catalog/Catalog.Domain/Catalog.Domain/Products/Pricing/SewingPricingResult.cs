namespace Catalog.Domain.Products.Pricing;

public sealed record SewingPricingResult(decimal PiecesPerShift, IReadOnlyList<SewingFabricPrice> ByFabric);
