using Catalog.Application.Features.Product.SheetImport.DTOs;

namespace Catalog.Api.Contracts.Products.Overview;

/// <summary>Products of the Calculator sheet (CAT-010); dryRun returns the report without saving.</summary>
public sealed record ImportSheetProductsRequest(
    IReadOnlyList<SheetSewingRow> Sewing,
    IReadOnlyList<SheetPpeRow> Ppe,
    bool DryRun = true,
    bool DeleteMissing = false);
