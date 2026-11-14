namespace Crm.Application.Features.Tasks.Commands.SetTaskUrgent;

public sealed class SetTaskUrgentValidator : AbstractValidator<SetTaskUrgentCommand>
{
    public SetTaskUrgentValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}
