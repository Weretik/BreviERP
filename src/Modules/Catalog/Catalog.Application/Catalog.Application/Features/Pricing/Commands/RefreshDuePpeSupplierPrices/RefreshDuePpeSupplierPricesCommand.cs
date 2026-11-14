namespace Catalog.Application.Features.Pricing.Commands.RefreshDuePpeSupplierPrices;

/// <summary>
/// The scheduled step of «Імпорт цін» (brevierp-sync): reads every website, sheet and cabinet source due today and saves the
/// changed prices; an empty list when nothing is due.
/// </summary>
public sealed record RefreshDuePpeSupplierPricesCommand(string Actor) : ICommand<Result<IReadOnlyList<PpeScheduledRefreshOutcome>>>;
