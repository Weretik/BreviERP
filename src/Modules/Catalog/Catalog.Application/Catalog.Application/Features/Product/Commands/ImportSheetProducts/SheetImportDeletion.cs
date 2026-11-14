namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts;

/// <param name="Type">Sewing or Ppe.</param>
public sealed record SheetImportDeletion(int Id, string Type, string? Code, string Name);
