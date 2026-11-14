using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;

namespace Crm.Application.Features.Leads.Queries.GetLead;

/// <summary>The card of one lead with its calls (R-07).</summary>
public sealed record GetLeadQuery(LeadActor Actor, long Id) : IQuery<Result<LeadDetailResponse>>;

public sealed class GetLeadValidator : AbstractValidator<GetLeadQuery>
{
    public GetLeadValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
