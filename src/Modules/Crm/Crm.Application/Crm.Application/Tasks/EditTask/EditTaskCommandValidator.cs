using Crm.Application.Tasks.Validation;

namespace Crm.Application.Tasks.EditTask;

public sealed class EditTaskCommandValidator : AbstractValidator<EditTaskCommand>
{
    public EditTaskCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().SetValidator(new SaveTaskInputValidator());
        RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input is not null);
    }
}
