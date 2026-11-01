namespace Reminders.Application.Calendar.RemoveNonWorkingDay;

public sealed class RemoveNonWorkingDayCommandValidator : AbstractValidator<RemoveNonWorkingDayCommand>
{
    public RemoveNonWorkingDayCommandValidator() => RuleFor(x => x.Date).NotEqual(default(DateOnly));
}
