using Reference.Application.Features.ProductSection.Import.DTOs;

namespace Reference.Application.Features.ProductSection.Import;

/// <param name="DryRun">Only compute the preview; nothing is saved.</param>
/// <param name="DeleteMissing">Delete sections that are not in the sheet.</param>
public sealed record ImportProductSectionsCommand(
    IReadOnlyList<ProductSectionImportRow> Rows,
    bool DryRun,
    bool DeleteMissing) : ICommand<Result<ProductSectionImportResult>>;
