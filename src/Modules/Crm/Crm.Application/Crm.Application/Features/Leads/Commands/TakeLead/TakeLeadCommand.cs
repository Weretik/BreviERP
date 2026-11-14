using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;

namespace Crm.Application.Features.Leads.Commands.TakeLead;

/// <summary>«Взяти собі» (R-04): a free lead becomes the actor's.</summary>
public sealed record TakeLeadCommand(LeadActor Actor, long Id) : ICommand<Result<LeadDetailResponse>>;

public sealed class TakeLeadValidator : AbstractValidator<TakeLeadCommand>
{
    public TakeLeadValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
