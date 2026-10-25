namespace Catalog.Domain.Pricing.Ppe;

public static class PpeCalculator
{
    public const string FormulaVersion = "ppe-sheet-v1";
    public static PpePriceCalculation Calculate(PpePricingInput input)
    {
        var blockers = new List<PpePricingBlocker>();
        if (input.SupplierCost is <= 0 || input.ExternalRetail is < 0 || input.ExternalWholesale is < 0)
            return new(null, [new("InvalidAmount", "offer")]);
        var retail = input.ExternalApproved && input.ExternalRetail is > 0 ? input.ExternalRetail : null;
        var externalWholesale = input.ExternalApproved && input.ExternalWholesale is > 0 ? input.ExternalWholesale : null;
        var needsFallback = retail is null || externalWholesale is null;
        if (needsFallback)
        {
            if (!input.SupplierApproved) blockers.Add(new("UnapprovedOffer", "offer"));
            if (input.SupplierCost is not > 0) blockers.Add(new("MissingSupplierCost", "supplierCost"));
            if (input.Rules.Count != 1) blockers.Add(new(input.Rules.Count == 0 ? "MissingRule" : "AmbiguousRule", "rule"));
            else if (input.Rules[0] is var rule && (rule.Retail < 0 || rule.Wholesale < 0 || rule.LargeWholesale < 0))
                blockers.Add(new("InvalidMarkup", "rule"));
        }
        if (blockers.Count > 0) return new(null, blockers);
        try
        {
            var rule = needsFallback ? input.Rules[0] : new PpeMarkups(0, 0, 0);
            var cost = input.SupplierCost ?? 0;
            decimal Round(decimal amount) => decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
            var prices = new PpePriceTiers(
                retail ?? Round(cost * (1 + rule.Retail)),
                retail.HasValue && externalWholesale.HasValue
                    ? Round(retail.Value / 2 + externalWholesale.Value / 2)
                    : Round(cost * (1 + rule.Wholesale)),
                externalWholesale ?? Round(cost * (1 + rule.LargeWholesale)));
            return new(prices, []);
        }
        catch (OverflowException) { return new(null, [new("NumericOverflow", "prices")]); }
    }
}
