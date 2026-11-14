using Reference.Application.Features.GarmentAccessory.Commands.ImportGarmentAccessories;

namespace Reference.Api.GarmentAccessory;

/// <summary>Rows of the Calculator sheet «Фурнітура»; dryRun returns the preview without saving.</summary>
public sealed record ImportGarmentAccessoriesRequest(
    IReadOnlyList<GarmentAccessoryImportRow> Rows,
    bool DryRun = true,
    bool DeactivateMissing = false);
