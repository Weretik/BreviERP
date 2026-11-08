using Crm.Application.Leads.Access;
using Crm.Application.Leads.Contracts;

namespace Crm.Application.Leads.GetMyDay;

/// <summary>«Мій день» of the signed-in manager (R-09).</summary>
public sealed record GetMyDayQuery(LeadActor Actor) : IQuery<Result<LeadMyDayResponse>>;

public sealed class GetMyDayQueryValidator : AbstractValidator<GetMyDayQuery>
{
    public GetMyDayQueryValidator() => RuleFor(x => x.Actor).NotNull();
}
