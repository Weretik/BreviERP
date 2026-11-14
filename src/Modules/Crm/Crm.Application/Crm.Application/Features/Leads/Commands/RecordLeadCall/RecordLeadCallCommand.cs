using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;

namespace Crm.Application.Features.Leads.Commands.RecordLeadCall;

/// <summary>«Зберегти → наступний» of «Мій день» (R-06…R-08).</summary>
public sealed record RecordLeadCallCommand(LeadActor Actor, long Id, RecordLeadCallInput Input)
    : ICommand<Result<LeadDetailResponse>>;
