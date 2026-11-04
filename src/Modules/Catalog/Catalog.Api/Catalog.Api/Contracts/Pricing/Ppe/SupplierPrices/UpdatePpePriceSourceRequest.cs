namespace Catalog.Api.Contracts.Pricing.Ppe.SupplierPrices;

/// <summary>Discount off the supplier price (0–90%) and, when chosen, the «Постачальники» row of the source.</summary>
public sealed record UpdatePpePriceSourceRequest(decimal DiscountPercent, int? SupplierId);
