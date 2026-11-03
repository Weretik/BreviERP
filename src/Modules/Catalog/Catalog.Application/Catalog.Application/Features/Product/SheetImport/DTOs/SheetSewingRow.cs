namespace Catalog.Application.Features.Product.SheetImport.DTOs;

/// <summary>
/// One sewing model of the Calculator tab «Товари» with its articles («Прайс на сайт»), materials («Комплектація
/// виробів») and operations («Трудовитрати виробів»). Names are Ukrainian only.
/// </summary>
/// <param name="Code">Model ID, MOD-….</param>
/// <param name="Section">Section name from Reference «Типи і розділи».</param>
public sealed record SheetSewingRow(
    int RowNumber,
    string Code,
    string Name,
    string? Section,
    string? Season,
    bool IsActive,
    string? CurrentSiteUrl,
    string? QuoteDescription,
    IReadOnlyList<SheetSewingArticleRow> Articles,
    IReadOnlyList<SheetSewingMaterialRow> Materials,
    IReadOnlyList<SheetSewingOperationRow> Operations);
