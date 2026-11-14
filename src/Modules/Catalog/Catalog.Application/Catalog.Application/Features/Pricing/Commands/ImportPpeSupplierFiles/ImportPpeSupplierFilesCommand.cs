using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.PpeSupplierPrices.Sources;

namespace Catalog.Application.Features.Pricing.Commands.ImportPpeSupplierFiles;

/// <summary>
/// Reads a file supplier's price files and shows what changes (<paramref name="DryRun"/>), or saves the changed
/// prices as new approved offer versions. The staff member sends the same files again to apply.
/// </summary>
public sealed record ImportPpeSupplierFilesCommand(
    string Key,
    IReadOnlyList<SupplierPriceFile> Files,
    bool DryRun,
    string Actor,
    string ActorName) : ICommand<Result<PpePriceImportResult>>;
