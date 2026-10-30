using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Quotes.Versions;

namespace Crm.Application.Quotes.PrepareQuote;

public static class QuoteReadyValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IReadOnlyList<ValidationError> Validate(QuoteVersion version)
    {
        var errors = new List<ValidationError>();
        if (string.IsNullOrWhiteSpace(version.RecipientName))
            errors.Add(new ValidationError("recipientName", "recipient_required"));
        if (version.Currency != "UAH" || version.TaxBasis != "no_extra_vat")
            errors.Add(new ValidationError("version", "unsupported_currency_or_tax_basis"));

        var included = version.Items.Where(x => x.Included).ToArray();
        if (included.Length == 0)
            errors.Add(new ValidationError("items", "no_included_items"));
        foreach (var item in included)
        {
            var visible = item.Variants.Where(x => x.Visible).ToArray();
            if (visible.Length == 0)
                errors.Add(new ValidationError($"items.{item.PublicId}", "no_visible_variants"));
            foreach (var variant in visible)
            {
                var field = $"variants.{variant.PublicId}";
                var tiers = variant.Tiers.Where(x => x.Visible).ToArray();
                if (tiers.Length == 0)
                    errors.Add(new ValidationError(field, "no_visible_price_tier"));
                else if (tiers.Any(x => x.Amount is null or <= 0 || x.Currency != "UAH"))
                    errors.Add(new ValidationError(field, "invalid_visible_price_tier"));
                try
                {
                    var basis = JsonSerializer.Deserialize<QuotePricePreview>(variant.PriceBasisJson, JsonOptions);
                    if (basis is null || basis.Blockers is null || basis.Blockers.Count > 0 ||
                        basis.Currency != "UAH" || basis.TaxBasis is not
                            ("manual_no_extra_vat" or "catalog_no_extra_vat" or "custom_no_extra_vat"))
                        errors.Add(new ValidationError(field, "pricing_basis_blocked"));
                }
                catch (JsonException)
                {
                    errors.Add(new ValidationError(field, "pricing_basis_invalid"));
                }
            }
        }
        return errors;
    }
}
