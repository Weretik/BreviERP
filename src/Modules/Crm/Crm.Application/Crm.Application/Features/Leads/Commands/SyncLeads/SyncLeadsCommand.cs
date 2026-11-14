using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Sheet;

namespace Crm.Application.Features.Leads.Commands.SyncLeads;

/// <summary>«Синхронізувати зараз» (R-03): the same run as the daily brevierp-sync step.</summary>
public sealed record SyncLeadsCommand : ICommand<Result<LeadSyncResponse>>;
