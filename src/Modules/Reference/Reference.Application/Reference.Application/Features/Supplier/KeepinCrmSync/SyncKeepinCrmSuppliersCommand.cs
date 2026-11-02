using Reference.Application.Contracts.KeepinCrm;
using Reference.Application.Features.Supplier.KeepinCrmSync.DTOs;

namespace Reference.Application.Features.Supplier.KeepinCrmSync;

/// <summary>«Оновити з KeepinCRM» on the admin page.</summary>
public sealed record SyncKeepinCrmSuppliersCommand : ICommand<Result<KeepinCrmSupplierSyncResult>>;

public sealed class SyncKeepinCrmSuppliersCommandHandler(KeepinCrmSupplierSync sync)
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
