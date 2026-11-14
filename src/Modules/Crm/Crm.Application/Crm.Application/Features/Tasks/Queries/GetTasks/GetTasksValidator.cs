using Crm.Domain.Tasks;

namespace Crm.Application.Features.Tasks.Queries.GetTasks;

public sealed class GetTasksValidator : AbstractValidator<GetTasksQuery>
{
    public const int MaxPageSize = 100;

    public GetTasksValidator()
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
