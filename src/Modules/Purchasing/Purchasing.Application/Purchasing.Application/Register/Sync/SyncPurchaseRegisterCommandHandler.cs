using Purchasing.Application.Contracts;

namespace Purchasing.Application.Register.Sync;

public sealed class SyncPurchaseRegisterCommandHandler(PurchaseRegisterSync sync)
    : ICommandHandler<SyncPurchaseRegisterCommand, Result<PurchaseSyncResponse>>
{
    public async ValueTask<Result<PurchaseSyncResponse>> Handle(SyncPurchaseRegisterCommand command, CancellationToken cancellationToken) =>
        await sync.RunAsync(cancellationToken);
}
