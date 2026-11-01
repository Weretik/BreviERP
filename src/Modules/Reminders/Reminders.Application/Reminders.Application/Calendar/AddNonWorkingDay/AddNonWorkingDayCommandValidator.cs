namespace Reminders.Application.Calendar.AddNonWorkingDay;

public sealed class AddNonWorkingDayCommandValidator : AbstractValidator<AddNonWorkingDayCommand>
{
    public AddNonWorkingDayCommandValidator()
    {
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Date).NotEqual(default(DateOnly)).When(x => x.Input is not null);
        RuleFor(x => x.Input.Note).MaximumLength(200).When(x => x.Input is not null);
    }
}
