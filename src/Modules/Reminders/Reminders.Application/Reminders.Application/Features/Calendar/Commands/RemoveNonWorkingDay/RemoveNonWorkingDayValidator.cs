namespace Reminders.Application.Features.Calendar.Commands.RemoveNonWorkingDay;

public sealed class RemoveNonWorkingDayValidator : AbstractValidator<RemoveNonWorkingDayCommand>
{
    public RemoveNonWorkingDayValidator() => RuleFor(x => x.Date).NotEqual(default(DateOnly));
}
