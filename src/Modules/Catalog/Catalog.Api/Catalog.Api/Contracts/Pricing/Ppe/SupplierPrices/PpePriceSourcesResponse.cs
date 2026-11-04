using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.List;

namespace Catalog.Api.Contracts.Pricing.Ppe.SupplierPrices;

/// <summary>«Імпорт цін» rows; <paramref name="CanChangeDiscounts"/> is true for administrators and the director.</summary>
public sealed record PpePriceSourcesResponse(bool CanChangeDiscounts, IReadOnlyList<PpePriceSourceView> Sources);
