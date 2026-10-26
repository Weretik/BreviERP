using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Evaluation;
using Catalog.Domain.Pricing.Ppe;
using CatalogImport.Source;

namespace CatalogImport.PpePricing.Verification;

internal static class PpePricingDiagnosticBuilder
{
    public static PpePricingDiagnostic Build(WorkbookRow row, PpePricingProductInput? input)
    {
        var strict = input is null ? null : PpeProductPricingEvaluator.Evaluate(input);
        // Diagnostic approval is in-memory only. No version writer or publication API is called.
        var arithmetic = input?.Offer is { } offer ? PpeCalculator.Calculate(new(offer.Values.SupplierCost, true,
            offer.Values.ExternalRetail, offer.Values.ExternalWholesale, true,
            input.Rule is { } rule ? [rule.Values.Markups] : [])) : null;
        return new(row.Text(0), input?.ProductId, input?.Offer?.Id, input?.Rule?.Id, PpeCalculator.FormulaVersion,
            arithmetic?.Prices, strict?.Calculation.Blockers ?? [new("MissingProduct", "product")],
            PpePricingParityComparer.Compare(row, arithmetic?.Prices));
    }
}
