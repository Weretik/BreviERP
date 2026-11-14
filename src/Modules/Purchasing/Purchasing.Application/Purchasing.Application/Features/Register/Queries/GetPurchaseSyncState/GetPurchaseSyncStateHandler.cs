using Purchasing.Application.Abstractions;
using Purchasing.Domain.Sync;

namespace Purchasing.Application.Features.Register.Queries.GetPurchaseSyncState;

public sealed class GetPurchaseSyncStateHandler(IPurchasingDbContext db)
    : IQueryHandler<GetPurchaseSyncStateQuery, Result<PurchaseSyncStateResponse>>
{
    public async ValueTask<Result<PurchaseSyncStateResponse>> Handle(GetPurchaseSyncStateQuery query, CancellationToken cancellationToken)
    {
        var state = await db.SyncStates.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == PurchaseSyncState.GoogleFinance, cancellationToken);
        return Result.Success(state is null
            ? new PurchaseSyncStateResponse(null, null, null, 0, 0)
            : new PurchaseSyncStateResponse(state.LastSuccessAt, state.LastAttemptAt, state.Error, state.RecordCount, state.FileCount));
    }
}
