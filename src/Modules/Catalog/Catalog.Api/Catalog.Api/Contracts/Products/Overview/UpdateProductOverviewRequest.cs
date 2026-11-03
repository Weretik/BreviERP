namespace Catalog.Api.Contracts.Products.Overview;

/// <summary>The «Товари» page fields of one product; every field is replaced.</summary>
public sealed record UpdateProductOverviewRequest(
    string? Code,
    int? SectionId,
    string? Season,
    bool IsActive,
    bool IsVerified,
    string? VerifiedOn,
    string? CurrentSiteUrl,
    string? QuoteDescription);
