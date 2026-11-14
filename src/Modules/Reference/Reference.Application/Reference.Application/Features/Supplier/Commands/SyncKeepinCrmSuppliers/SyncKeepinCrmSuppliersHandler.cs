using Reference.Application.Abstractions;

namespace Reference.Application.Features.Supplier.Commands.SyncKeepinCrmSuppliers;

public sealed class SyncKeepinCrmSuppliersHandler(KeepinCrmSupplierSync sync)
    : ICommandHandler<SyncKeepinCrmSuppliersCommand, Result<KeepinCrmSupplierSyncResult>>
{
    public async ValueTask<Result<KeepinCrmSupplierSyncResult>> Handle(
        SyncKeepinCrmSuppliersCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            return Result.Success(await sync.RunAsync(cancellationToken));
        }
        catch (KeepinCrmUnavailableException error)
        {
            return Result.Unavailable(error.Message);
        }
    }
}
