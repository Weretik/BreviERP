namespace Catalog.Domain.Products;

/// <summary>
/// The working fields of one row on the «Товари» page, kept in the product's catalog profile: section from
/// Reference «Типи і розділи», season «Літо»/«Зима» (sewing only), activity, the link to the product page on the current public site, and the short description for a quote (КП).
/// </summary>
public sealed record ProductOverviewDetails(
    int? SectionId,
    string? Season,
    bool IsActive,
    string? CurrentSiteUrl,
    string? QuoteDescription);
