
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients.Results;

namespace Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;

/// <summary>«Оновити з KeepinCRM» on the clients tab; <paramref name="DryRun"/> previews without saving.</summary>
public sealed record SyncKeepinCrmClientsCommand(bool DryRun, bool RemoveUnlinked)
    : ICommand<Result<KeepinCrmClientSyncResult>>;
