using Catalog.Application.Features.Product.Commands.ImportSheetProducts;
using Catalog.Application.Features.Product.Commands.ImportSheetProducts.Plan;

namespace Catalog.Application.Abstractions.Products;

/// <summary>Reads the stored products for the sheet import and writes an import plan in one transaction.</summary>
public interface ISheetProductImportStore
{
    /// <summary>All products, tracked, with profile, articles and composition, and the saved calculation usage.</summary>
    Task<SheetImportStoredState> LoadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Deletes, then creates and updates, then replaces compositions and appends PPE price versions, all in one
    /// database transaction. Conflict when the database rejects a change; then nothing is saved.
    /// </summary>
    Task<Result> ApplyAsync(SheetImportPlan plan, string actor, CancellationToken cancellationToken);
}
