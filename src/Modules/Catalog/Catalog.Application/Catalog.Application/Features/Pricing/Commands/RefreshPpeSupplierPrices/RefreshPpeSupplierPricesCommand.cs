using Catalog.Application.Features.Pricing.Commands.ImportPpeSupplierFiles;

namespace Catalog.Application.Features.Pricing.Commands.RefreshPpeSupplierPrices;

/// <summary>
/// «Оновити зараз»: reads a website or Google Sheets source now and shows what changes (<paramref name="DryRun"/>),
/// or saves the changed prices the same way a file import does.
/// </summary>
public sealed record RefreshPpeSupplierPricesCommand(string Key, bool DryRun, string Actor, string ActorName)
    : ICommand<Result<PpePriceImportResult>>;
