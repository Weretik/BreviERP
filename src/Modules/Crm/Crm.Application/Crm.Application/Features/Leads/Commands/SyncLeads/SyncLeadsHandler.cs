using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Sheet;

namespace Crm.Application.Features.Leads.Commands.SyncLeads;

public sealed class SyncLeadsHandler(LeadSheetSync sync) : ICommandHandler<SyncLeadsCommand, Result<LeadSyncResponse>>
{
    public async ValueTask<Result<LeadSyncResponse>> Handle(SyncLeadsCommand command,
        CancellationToken cancellationToken) => await sync.RunAsync(cancellationToken);
}
