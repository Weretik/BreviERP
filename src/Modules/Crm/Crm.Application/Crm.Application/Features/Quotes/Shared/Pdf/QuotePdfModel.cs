using Crm.Domain.Quotes.Settings;

namespace Crm.Application.Features.Quotes.Shared.Pdf;

/// <summary>
/// Everything a client may see in the КП PDF (R-14). Internal names, manager notes, suppliers, manual price
/// reasons and hidden prices are not part of it, so the renderer cannot leak them.
/// </summary>
public sealed record QuotePdfModel(
    string Number,
    DateOnly Date,
    string Title,
    QuotePdfCompany Company,
    string? RecipientName,
    string? ClientCompanyName,
    string? Intro,
    IReadOnlyList<QuotePdfItem> Items,
    QuotePdfTiers Tiers,
    string? ClientComment,
    string? VatNotice,
    string? CtaTitle,
    string? CtaText,
    string? CtaContact,
    QuotePdfContact Manager,
    QuotePdfLabels Labels,
    string? ContactLine);

public sealed record QuotePdfCompany(
    string Name,
    string? Edrpou,
    string? TaxId,
    string? Iban,
    string? Address,
    string? Phone,
    string? Email,
    string? Site,
    string? LogoUrl);

public sealed record QuotePdfContact(string Name, string? Phone, string? Email);

public sealed record QuotePdfItem(
    string Name,
    string? Code,
    string? Subtitle,
    string? Season,
    string? Description,
    string? PhotoUrl,
    string? SiteUrl,
    string? OptionsTitle,
    IReadOnlyList<QuotePdfOption> Options);

/// <summary>One price tile: the quantity above, the price below (R-35).</summary>
public sealed record QuotePdfPriceTile(string Label, decimal Price);

/// <summary>A price is null when it is hidden from the client.</summary>
public sealed record QuotePdfOption(
    string Title,
    string? Details,
    bool IsRecommended,
    decimal? Retail,
    decimal? Wholesale,
    decimal? Large)
{
    /// <summary>The prices the client sees, shifted left without gaps.</summary>
    public IReadOnlyList<QuotePdfPriceTile> Tiles => QuotePdfPrices.Tiles(Retail, Wholesale, Large);
}

/// <summary>Which quantity columns at least one row shows.</summary>
public sealed record QuotePdfTiers(bool Retail, bool Wholesale, bool Large);
