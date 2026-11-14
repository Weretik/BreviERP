namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts;

/// <summary>A labour line of «Трудовитрати виробів». The same operation may appear more than once.</summary>
/// <param name="Article">Must be null: operations are shared by all articles of the model.</param>
/// <param name="Code">Operation code, OP-….</param>
public sealed record SheetSewingOperationRow(
    int RowNumber,
    string? Article,
    string? Code,
    decimal? Quantity,
    decimal? ManualMinutes,
    string? Comment);
