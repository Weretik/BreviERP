namespace Catalog.Domain.Products.ValueObjects;

/// <summary>
/// The working fields of one row on the «Товари» page, kept in the product's catalog profile: section from
/// Reference «Типи і розділи», season «Літо»/«Зима» (sewing only), activity, the director's check and its date
/// (dd.MM.yyyy), the link to the product page on the current public site, and the short description for a quote (КП).
/// </summary>
public sealed record ProductOverviewDetails(
    int? SectionId,
    string? Season,
    bool IsActive,
    bool IsVerified,
    string? VerifiedOn,
    string? CurrentSiteUrl,
    string? QuoteDescription);
