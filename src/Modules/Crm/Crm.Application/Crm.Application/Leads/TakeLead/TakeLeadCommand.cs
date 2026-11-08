using Crm.Application.Leads.Access;
using Crm.Application.Leads.Contracts;

namespace Crm.Application.Leads.TakeLead;

/// <summary>«Взяти собі» (R-04): a free lead becomes the actor's.</summary>
public sealed record TakeLeadCommand(LeadActor Actor, long Id) : ICommand<Result<LeadDetailResponse>>;

public sealed class TakeLeadCommandValidator : AbstractValidator<TakeLeadCommand>
{
    public TakeLeadCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
