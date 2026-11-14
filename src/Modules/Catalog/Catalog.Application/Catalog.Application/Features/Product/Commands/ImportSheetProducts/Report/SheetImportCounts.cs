namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts.Report;

/// <summary>Rows of one product type that create, change or keep a product.</summary>
public sealed record SheetImportCounts(int Created, int Updated, int Unchanged);
