using Catalog.Application.Features.Product.Commands.ImportSheetProducts;
using Catalog.Application.Features.Product.Commands.ImportSheetProducts.Rows;

namespace Catalog.Api.Endpoints.Overview.ImportSheetProducts;

/// <summary>Products of the Calculator sheet (CAT-010); dryRun returns the report without saving.</summary>
public sealed record ImportSheetProductsRequest(
    IReadOnlyList<SheetSewingRow> Sewing,
    IReadOnlyList<SheetPpeRow> Ppe,
    bool DryRun = true,
    bool DeleteMissing = false);
