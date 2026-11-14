using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;

namespace Crm.Application.Features.Leads.Queries.GetMyDay;

/// <summary>«Мій день» of the signed-in manager (R-09).</summary>
public sealed record GetMyDayQuery(LeadActor Actor) : IQuery<Result<LeadMyDayResponse>>;

public sealed class GetMyDayValidator : AbstractValidator<GetMyDayQuery>
{
    public GetMyDayValidator() => RuleFor(x => x.Actor).NotNull();
}
