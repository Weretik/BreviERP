namespace Reminders.Application.Occurrences.GetOccurrences;

public sealed class GetReminderOccurrencesQueryValidator : AbstractValidator<GetReminderOccurrencesQuery>
{
    public GetReminderOccurrencesQueryValidator()
    {
        RuleFor(x => x.Status).Must(x => x is null or "open" or "done").WithMessage("Статус має бути open або done.");
        RuleFor(x => x.TaskId).GreaterThan(0).When(x => x.TaskId is not null);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
