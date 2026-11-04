using Crm.Domain.Tasks;

namespace Crm.Application.Tasks.GetTasks;

public sealed class GetTasksQueryValidator : AbstractValidator<GetTasksQuery>
{
    public const int MaxPageSize = 100;

    public GetTasksQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Search).MaximumLength(ManagerTaskLimits.TitleMaxLength);
        RuleFor(x => x.Filter)
            .Must(x => !x!.Any(char.IsDigit) && Enum.TryParse<TaskListFilter>(x, false, out var filter) &&
                Enum.IsDefined(filter))
            .When(x => !string.IsNullOrEmpty(x.Filter)).WithMessage("Невідомий фільтр завдань.");
        RuleFor(x => x.AssigneeUserId).GreaterThan(0).When(x => x.AssigneeUserId is not null);
        RuleFor(x => x.Page).InclusiveBetween(1, 100_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
