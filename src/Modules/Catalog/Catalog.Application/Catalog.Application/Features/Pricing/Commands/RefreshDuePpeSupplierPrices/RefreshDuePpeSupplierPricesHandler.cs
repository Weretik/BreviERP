namespace Catalog.Application.Features.Pricing.Commands.RefreshDuePpeSupplierPrices;

public sealed class RefreshDuePpeSupplierPricesHandler(PpeScheduledPriceRefresh refresh)
    : ICommandHandler<RefreshDuePpeSupplierPricesCommand, Result<IReadOnlyList<PpeScheduledRefreshOutcome>>>
{
    public async ValueTask<Result<IReadOnlyList<PpeScheduledRefreshOutcome>>> Handle(
        RefreshDuePpeSupplierPricesCommand command, CancellationToken cancellationToken) =>
        refresh.DueToday().Count == 0
            ? Result.Success<IReadOnlyList<PpeScheduledRefreshOutcome>>([])
            : Result.Success(await refresh.RunDueAsync(command.Actor, cancellationToken));
}
