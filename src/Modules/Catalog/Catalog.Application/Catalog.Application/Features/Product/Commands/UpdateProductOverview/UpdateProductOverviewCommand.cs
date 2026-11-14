namespace Catalog.Application.Features.Product.Commands.UpdateProductOverview;

/// <summary>
/// Replaces the «Товари» page fields of one product. <paramref name="Season"/> is «Літо», «Зима» or null (sewing
/// only).
/// </summary>
public sealed record UpdateProductOverviewCommand(
    int Id,
    string? Code,
    int? SectionId,
    string? Season,
    bool IsActive,
    string? CurrentSiteUrl,
    string? QuoteDescription) : ICommand<Result>;
