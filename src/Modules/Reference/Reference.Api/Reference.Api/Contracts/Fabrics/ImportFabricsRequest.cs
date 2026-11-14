using Reference.Application.Features.Fabric.Commands.ImportFabrics;

namespace Reference.Api.Contracts.Fabrics;

/// <summary>Rows of the Calculator sheet «Тканини»; dryRun returns the preview without saving.</summary>
public sealed record ImportFabricsRequest(
    IReadOnlyList<FabricImportRow> Rows,
    bool DryRun = true,
    bool DeactivateMissing = false);
