using Crm.Application.Features.Tasks.Shared.Validation;

namespace Crm.Application.Features.Tasks.Commands.EditTask;

public sealed class EditTaskValidator : AbstractValidator<EditTaskCommand>
{
    public EditTaskValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().SetValidator(new SaveTaskInputValidator());
        RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input is not null);
    }
}
