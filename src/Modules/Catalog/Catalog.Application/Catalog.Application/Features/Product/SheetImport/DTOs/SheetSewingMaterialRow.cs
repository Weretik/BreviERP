namespace Catalog.Application.Features.Product.SheetImport.DTOs;

/// <summary>A material line of «Комплектація виробів».</summary>
/// <param name="Article">Sale SKU of the article the line belongs to; null for a line shared by all articles.</param>
/// <param name="Kind">Fabric or Accessory.</param>
/// <param name="Code">FAB-… for a fabric, FUR-… for an accessory.</param>
/// <param name="Unit">Metre or Piece.</param>
public sealed record SheetSewingMaterialRow(
    int RowNumber,
    string? Article,
    string? Kind,
    string? Code,
    decimal? Quantity,
    string? Unit,
    string? Comment);
