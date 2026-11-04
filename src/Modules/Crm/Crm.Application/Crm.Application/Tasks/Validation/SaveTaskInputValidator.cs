using Crm.Application.Tasks.Contracts;
using Crm.Domain.Tasks;

namespace Crm.Application.Tasks.Validation;

/// <summary>Field rules of the side panel (TSK-001 R-03).</summary>
public sealed class SaveTaskInputValidator : AbstractValidator<SaveTaskInput>
{
    public SaveTaskInputValidator()
    {
        RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Title).NotEmpty().WithMessage("Вкажіть, що зробити.")
            .MaximumLength(ManagerTaskLimits.TitleMaxLength);
        RuleFor(x => x.Title).Must(x => !string.IsNullOrWhiteSpace(x)).When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Вкажіть, що зробити.");
        RuleFor(x => x.Description).MaximumLength(ManagerTaskLimits.DescriptionMaxLength);
        RuleFor(x => x.AssigneeUserId).GreaterThan(0).WithMessage("Оберіть менеджера.");
        RuleFor(x => x.DueAt).NotEqual(default(DateTimeOffset)).WithMessage("Вкажіть термін.")
            .Must(x => x.Year is >= 2020 and <= 2100).WithMessage("Термін поза допустимими датами.");
        RuleFor(x => x.CustomerPublicId).NotEqual(Guid.Empty).When(x => x.CustomerPublicId is not null);
        RuleFor(x => x.Steps!.Count).LessThanOrEqualTo(ManagerTaskLimits.MaxSteps)
            .When(x => x.Steps is not null).WithMessage($"Не більше {ManagerTaskLimits.MaxSteps} кроків.");
        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(x => x.Text).Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Крок не може бути порожнім.")
                .MaximumLength(ManagerTaskLimits.StepTextMaxLength);
        });
    }
}
