namespace Catalog.Api.Endpoints.Ppe.UpdatePpePriceSource;

/// <summary>Discount off the supplier price (0–90%) and, when chosen, the «Постачальники» row of the source.</summary>
public sealed record UpdatePpePriceSourceRequest(decimal DiscountPercent, int? SupplierId);
