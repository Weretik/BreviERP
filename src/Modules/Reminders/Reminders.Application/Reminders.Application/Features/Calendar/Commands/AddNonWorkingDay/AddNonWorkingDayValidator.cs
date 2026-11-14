namespace Reminders.Application.Features.Calendar.Commands.AddNonWorkingDay;

public sealed class AddNonWorkingDayValidator : AbstractValidator<AddNonWorkingDayCommand>
{
    public AddNonWorkingDayValidator()
    {
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Date).NotEqual(default(DateOnly)).When(x => x.Input is not null);
        RuleFor(x => x.Input.Note).MaximumLength(200).When(x => x.Input is not null);
    }
}
