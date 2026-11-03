namespace Catalog.Application.Features.Product.Overview.GetList.DTOs;

/// <summary>One row of the «Товари» page; <paramref name="VerifiedOn"/> is the check date text (dd.MM.yyyy).</summary>
public sealed record ProductOverviewRow(
    int Id,
    string Type,
    string? Code,
    string Name,
    int? SectionId,
    string? Season,
    bool IsActive,
    bool IsVerified,
    string? VerifiedOn,
    string? CurrentSiteUrl,
    string? QuoteDescription,
    DateTimeOffset UpdatedAtUtc);
