using Catalog.Application.Contracts.Pricing.Ppe;
namespace Catalog.Application.Features.Pricing.Ppe.Validation;
public static class PpeVersionValidation
{
    public static bool Request(Guid requestId, Guid? expected, string actor)
        => requestId != Guid.Empty && expected != Guid.Empty && Text(actor, 200);
    public static bool Rule(PpeRuleValues? value)
        => value is not null && Text(value.Code, 200) && Text(value.Source, 1000) && Text(value.SourceDate, 100)
            && value.Markups is not null && Fraction(value.Markups.Retail) && Fraction(value.Markups.Wholesale) && Fraction(value.Markups.LargeWholesale);
    public static bool Offer(PpeOfferValues? value)
        => value is not null && (value.RuleCode is null || Text(value.RuleCode, 200))
            && Text(value.Source, 1000) && Text(value.SourceDate, 100)
            && Amount(value.SupplierCost, false) && Amount(value.ExternalRetail, true) && Amount(value.ExternalWholesale, true);
    private static bool Text(string? value, int length) => !string.IsNullOrWhiteSpace(value) && value.Length <= length && value == value.Trim();
    private static bool Fraction(decimal value) => value >= 0 && value < 1000000000000m && decimal.Round(value, 8) == value;
    private static bool Amount(decimal? value, bool zero) => value is null || (value >= (zero ? 0 : .000001m)
        && value < 1000000000000m && decimal.Round(value.Value, 6) == value);
}
