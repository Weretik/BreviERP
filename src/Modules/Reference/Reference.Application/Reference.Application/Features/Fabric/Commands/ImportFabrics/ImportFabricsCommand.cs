using Reference.Application.Features.SheetImport.Shared;

namespace Reference.Application.Features.Fabric.Commands.ImportFabrics;

/// <param name="DryRun">Only compute the preview; nothing is saved.</param>
/// <param name="DeactivateMissing">Mark fabrics that are not in the sheet as not used («Не використовується»).</param>
public sealed record ImportFabricsCommand(
    IReadOnlyList<FabricImportRow> Rows,
    bool DryRun,
    bool DeactivateMissing) : ICommand<Result<ReferenceImportResult>>;
