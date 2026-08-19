namespace Catalog.Domain.Products.Pricing;

public sealed record SewingPricingInput(
    decimal MetersPerProduct,
    IReadOnlyList<SewingFabricCost> Fabrics,
    decimal AccessoriesCost,
    IReadOnlyList<decimal> OperationMinutes,
    SewingWorkshopCosts WorkshopCosts,
    SewingProfitMargins ProfitMargins);
