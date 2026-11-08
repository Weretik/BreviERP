using Crm.Application.Leads.Contracts;
using Crm.Application.Leads.Sheet;

namespace Crm.Application.Leads.Sync;

/// <summary>«Синхронізувати зараз» (R-03): the same run as the daily brevierp-sync step.</summary>
public sealed record SyncLeadsCommand : ICommand<Result<LeadSyncResponse>>;

public sealed class SyncLeadsCommandHandler(LeadSheetSync sync) : ICommandHandler<SyncLeadsCommand, Result<LeadSyncResponse>>
{
    public async ValueTask<Result<LeadSyncResponse>> Handle(SyncLeadsCommand command,
        CancellationToken cancellationToken) => await sync.RunAsync(cancellationToken);
}
