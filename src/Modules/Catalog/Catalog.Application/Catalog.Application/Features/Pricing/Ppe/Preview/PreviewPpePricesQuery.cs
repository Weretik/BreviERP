using Catalog.Domain.Pricing.Ppe;

namespace Catalog.Application.Features.Pricing.Ppe.Preview;

/// <summary>
/// The three PPE prices for a supplier price and rule before anything is saved (page «Новий ЗІЗ», CAT-009).
/// Uses the same calculator as the saved card; the rule must exist.
/// </summary>
public sealed record PreviewPpePricesQuery(string RuleCode, decimal SupplierPrice) : IQuery<Result<PpePriceCalculation>>;
