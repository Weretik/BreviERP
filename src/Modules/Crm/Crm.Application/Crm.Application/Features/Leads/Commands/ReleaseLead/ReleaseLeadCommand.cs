using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Access;

namespace Crm.Application.Features.Leads.Commands.ReleaseLead;

/// <summary>«Повернути у вільні» (R-04): the owner, or Director/Admin for anybody's lead.</summary>
public sealed record ReleaseLeadCommand(LeadActor Actor, long Id, int Version) : ICommand<Result<LeadDetailResponse>>;

public sealed class ReleaseLeadValidator : AbstractValidator<ReleaseLeadCommand>
{
    public ReleaseLeadValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}
