namespace Catalog.Api.Endpoints.Overview.UpdateProductOverview;

/// <summary>The «Товари» page fields of one product; every field is replaced.</summary>
public sealed record UpdateProductOverviewRequest(
    string? Code,
    int? SectionId,
    string? Season,
    bool IsActive,
    string? CurrentSiteUrl,
    string? QuoteDescription);
