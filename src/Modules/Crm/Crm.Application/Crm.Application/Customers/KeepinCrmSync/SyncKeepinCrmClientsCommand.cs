using Crm.Application.Customers.KeepinCrmSync.DTOs;

namespace Crm.Application.Customers.KeepinCrmSync;

/// <summary>«Оновити з KeepinCRM» on the clients tab; <paramref name="DryRun"/> previews without saving.</summary>
public sealed record SyncKeepinCrmClientsCommand(bool DryRun, bool RemoveUnlinked)
    : ICommand<Result<KeepinCrmClientSyncResult>>;

public sealed class SyncKeepinCrmClientsCommandHandler(KeepinCrmClientSync sync)
    : ICommandHandler<SyncKeepinCrmClientsCommand, Result<KeepinCrmClientSyncResult>>
{
    public async ValueTask<Result<KeepinCrmClientSyncResult>> Handle(
        SyncKeepinCrmClientsCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            return Result.Success(await sync.RunAsync(command.DryRun, command.RemoveUnlinked, cancellationToken));
        }
        catch (KeepinCrmClientsUnavailableException error)
        {
            return Result.Unavailable(error.Message);
        }
    }
}
