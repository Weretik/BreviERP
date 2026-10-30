using System.Globalization;
using Crm.Application.Abstractions.Documents;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Versions;

namespace Crm.Application.Documents.Projection;

/// <summary>Allowlisted customer-facing text from one saved quote and pinned settings versions.</summary>
public static class QuotePublicDocumentProjectionBuilder
{
    private static readonly (string Code, string Label)[] TierOrder =
    [
        ("retail", "Роздрібна ціна"),
        ("wholesale", "Гуртова ціна"),
        ("largeWholesale", "Великий гурт")
    ];

    public static PublicDocumentProjection Build(QuoteVersion version, IssuerSettingsVersion issuer,
        DocumentTemplateVersion template, string number, string publicContactName, PublicDocumentImage? logo = null)
    {
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(issuer);
        ArgumentNullException.ThrowIfNull(template);
        if (string.IsNullOrWhiteSpace(number) || number.Length > 80 ||
            string.IsNullOrWhiteSpace(publicContactName) || publicContactName.Length > 200 ||
            version.Currency != "UAH" || template.LayoutKey != "standard")
            throw new InvalidOperationException("public_document_input_invalid");
        if ((issuer.LogoAssetKey is null) != (logo is null))
            throw new InvalidOperationException("public_logo_asset_missing_or_unexpected");

        var paragraphs = new List<string>
        {
            $"Одержувач: {version.RecipientName}",
            template.OpeningText,
            $"Продавець: {issuer.LegalName}",
            $"ЄДРПОУ: {issuer.Edrpou}",
            $"Податковий номер: {issuer.TaxId}",
            $"Адреса: {issuer.Address}",
            $"IBAN: {issuer.Iban}",
            $"Телефон: {issuer.Phone}",
            $"Email: {issuer.Email}",
            $"Сайт: {issuer.Site}",
            template.VatNotice
        };
        if (!string.IsNullOrWhiteSpace(issuer.BankName)) paragraphs.Add($"Банк: {issuer.BankName}");
        if (!string.IsNullOrWhiteSpace(issuer.Mfo)) paragraphs.Add($"МФО: {issuer.Mfo}");

        var groups = new List<PublicDocumentTextGroup> { new(paragraphs.ToArray()) };
        var included = version.Items.Where(x => x.Included).OrderBy(x => x.SortOrder).ToArray();
        if (included.Length == 0) throw new InvalidOperationException("public_items_missing");
        foreach (var item in included)
        {
            var firstParagraph = paragraphs.Count;
            var visible = item.Variants.Where(x => x.Visible).OrderBy(x => x.SortOrder).ToArray();
            if (visible.Length == 0) throw new InvalidOperationException("public_variants_missing");
            paragraphs.Add(item.Title);
            if (!string.IsNullOrWhiteSpace(item.PublicDescription)) paragraphs.Add(item.PublicDescription);
            if (!string.IsNullOrWhiteSpace(item.PublicComment)) paragraphs.Add(item.PublicComment);
            foreach (var variant in visible)
            {
                paragraphs.Add(variant.Title);
                var visibleTiers = 0;
                foreach (var (code, label) in TierOrder)
                {
                    var tier = variant.Tiers.Single(x => x.Code == code);
                    if (!tier.Visible) continue;
                    if (tier.Amount is not > 0 || tier.Currency != "UAH")
                        throw new InvalidOperationException("public_tier_price_missing");
                    paragraphs.Add($"{label}: {tier.Amount.Value.ToString("0.00", CultureInfo.InvariantCulture)} грн");
                    visibleTiers++;
                }
                if (visibleTiers == 0) throw new InvalidOperationException("public_tiers_missing");
            }
            groups.Add(new PublicDocumentTextGroup(paragraphs.Skip(firstParagraph).ToArray()));
        }

        var ctaStart = paragraphs.Count;
        paragraphs.Add(template.CtaTitle);
        paragraphs.Add(template.CtaText);
        paragraphs.Add(template.CtaContactTemplate.Replace("{managerName}", publicContactName.Trim(), StringComparison.Ordinal));
        if (paragraphs.Count > 500 || paragraphs.Any(x => x.Length > 8000))
            throw new InvalidOperationException("public_document_too_large");
        groups.Add(new PublicDocumentTextGroup(paragraphs.Skip(ctaStart).ToArray()));
        return new PublicDocumentProjection($"{template.Title} № {number}", paragraphs, [], logo, TextGroups: groups);
    }
}
