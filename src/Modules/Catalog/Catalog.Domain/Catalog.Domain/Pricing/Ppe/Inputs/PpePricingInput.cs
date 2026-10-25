namespace Catalog.Domain.Pricing.Ppe;
public sealed record PpePricingInput(decimal? SupplierCost, bool SupplierApproved, decimal? ExternalRetail,
    decimal? ExternalWholesale, bool ExternalApproved, IReadOnlyList<PpeMarkups> Rules);
