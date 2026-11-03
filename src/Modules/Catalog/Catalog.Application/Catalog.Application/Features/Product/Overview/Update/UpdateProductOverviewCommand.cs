namespace Catalog.Application.Features.Product.Overview.Update;

/// <summary>
/// Replaces the «Товари» page fields of one product. <paramref name="Season"/> is «Літо», «Зима» or null (sewing
/// only); a checked product without <paramref name="VerifiedOn"/> keeps its date or gets today (Kyiv, dd.MM.yyyy).
/// </summary>
public sealed record UpdateProductOverviewCommand(
    int Id,
    string? Code,
    int? SectionId,
    string? Season,
    bool IsActive,
    bool IsVerified,
    string? VerifiedOn,
    string? CurrentSiteUrl,
    string? QuoteDescription) : ICommand<Result>;
