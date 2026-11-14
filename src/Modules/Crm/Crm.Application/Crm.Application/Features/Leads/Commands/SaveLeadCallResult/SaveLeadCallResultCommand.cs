using Crm.Domain.Leads;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;
using Crm.Application.Features.Leads.Shared.Mapping;
using Crm.Application.Features.Leads.Shared.Persistence;

namespace Crm.Application.Features.Leads.Commands.SaveLeadCallResult;

/// <summary>
/// «Довідники › Результати дзвінка» (R-11): adds (<paramref name="Id"/> null) or changes a result button. A result is
/// never deleted, only switched off, so the calls recorded with it keep their text.
/// </summary>
public sealed record SaveLeadCallResultCommand(long? Id, SaveLeadCallResultInput Input)
    : ICommand<Result<LeadCallResultResponse>>;
