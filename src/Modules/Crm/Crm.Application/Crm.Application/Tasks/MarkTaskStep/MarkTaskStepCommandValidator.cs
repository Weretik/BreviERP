namespace Crm.Application.Tasks.MarkTaskStep;

public sealed class MarkTaskStepCommandValidator : AbstractValidator<MarkTaskStepCommand>
{
    public MarkTaskStepCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.StepId).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}
