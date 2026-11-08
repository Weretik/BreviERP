using Crm.Application.Leads.Access;
using Crm.Application.Leads.Contracts;

namespace Crm.Application.Leads.RecordLeadCall;

/// <summary>«Зберегти → наступний» of «Мій день» (R-06…R-08).</summary>
public sealed record RecordLeadCallCommand(LeadActor Actor, long Id, RecordLeadCallInput Input)
    : ICommand<Result<LeadDetailResponse>>;
