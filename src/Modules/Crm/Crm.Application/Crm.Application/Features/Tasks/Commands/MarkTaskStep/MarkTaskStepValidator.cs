namespace Crm.Application.Features.Tasks.Commands.MarkTaskStep;

public sealed class MarkTaskStepValidator : AbstractValidator<MarkTaskStepCommand>
{
    public MarkTaskStepValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.StepId).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}
