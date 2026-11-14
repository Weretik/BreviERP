using Crm.Application.Abstractions.Customers;

namespace Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;

public sealed class SyncKeepinCrmClientsHandler(KeepinCrmClientSync sync)
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
