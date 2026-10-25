using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Validation;
using CatalogImport.PpePricing.Source;
namespace CatalogImport.PpePricing.Planning;
public static class PpePricingImportPlanner
{
    public static PpePricingImportPlan Build(PpePricingSource source, string hash)
    {
        var provenance = $"sheet:{source.SpreadsheetId}:ppe:{hash}";
        var rules = source.RuleRows().Select(x => new PpeRuleValues(x.Text(0),new(x.Decimal(1)!.Value,x.Decimal(2)!.Value,x.Decimal(3)!.Value),provenance,source.CapturedAt)).ToArray();
        var offers = source.ProductRows().ToDictionary(x=>x.Text(0), x=> new PpeOfferValues(
            string.IsNullOrWhiteSpace(x.Text(13)) ? null : x.Text(13), x.Decimal(7), x.Decimal(17), x.Decimal(16),
            provenance, string.IsNullOrWhiteSpace(x.Text(12)) ? source.CapturedAt : x.Text(12),x.Flag(11)),StringComparer.Ordinal);
        if (rules.Select(x=>x.Code).Distinct(StringComparer.Ordinal).Count()!=rules.Length
            || rules.Any(x=>!PpeVersionValidation.Rule(x)) || offers.Values.Any(x=>!PpeVersionValidation.Offer(x)))
            throw new InvalidDataException("PPE source contains invalid or duplicate rule/offer inputs.");
        return new(rules,offers);
    }
}
