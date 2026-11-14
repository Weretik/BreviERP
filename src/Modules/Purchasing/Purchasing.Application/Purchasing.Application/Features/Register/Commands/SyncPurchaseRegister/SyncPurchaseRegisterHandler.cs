
namespace Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;

public sealed class SyncPurchaseRegisterHandler(PurchaseRegisterSync sync)
    : ICommandHandler<SyncPurchaseRegisterCommand, Result<PurchaseSyncResponse>>
{
    public async ValueTask<Result<PurchaseSyncResponse>> Handle(SyncPurchaseRegisterCommand command, CancellationToken cancellationToken) =>
        await sync.RunAsync(cancellationToken);
}
