using Crm.Application.Leads.Access;
using Crm.Application.Leads.Contracts;

namespace Crm.Application.Leads.ReleaseLead;

/// <summary>«Повернути у вільні» (R-04): the owner, or Director/Admin for anybody's lead.</summary>
public sealed record ReleaseLeadCommand(LeadActor Actor, long Id, int Version) : ICommand<Result<LeadDetailResponse>>;

public sealed class ReleaseLeadCommandValidator : AbstractValidator<ReleaseLeadCommand>
{
    public ReleaseLeadCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}
