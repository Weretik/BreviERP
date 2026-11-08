using Crm.Application.Leads.Access;
using Crm.Application.Leads.Contracts;

namespace Crm.Application.Leads.GetLead;

/// <summary>The card of one lead with its calls (R-07).</summary>
public sealed record GetLeadQuery(LeadActor Actor, long Id) : IQuery<Result<LeadDetailResponse>>;

public sealed class GetLeadQueryValidator : AbstractValidator<GetLeadQuery>
{
    public GetLeadQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
