using Crm.Application.Features.Tasks.Shared.Validation;

namespace Crm.Application.Features.Tasks.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.Status).Must(TaskStatusParser.IsKnown).WithMessage("Невідомий статус завдання.");
    }
}
