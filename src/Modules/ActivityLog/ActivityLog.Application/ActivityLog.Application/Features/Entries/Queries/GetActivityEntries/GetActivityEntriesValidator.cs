using ActivityLog.Contracts.Activity;

namespace ActivityLog.Application.Features.Entries.Queries.GetActivityEntries;

public sealed class GetActivityEntriesValidator : AbstractValidator<GetActivityEntriesQuery>
{
    public GetActivityEntriesValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Номер сторінки має бути від 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, GetActivityEntriesQuery.MaxPageSize)
            .WithMessage($"На сторінці може бути від 1 до {GetActivityEntriesQuery.MaxPageSize} записів.");
        RuleFor(x => x.ActorId).GreaterThan(0).When(x => x.ActorId is not null)
            .WithMessage("Невідомий працівник.");
        RuleFor(x => x.Role).MaximumLength(64).WithMessage("Назва ролі задовга.");
        RuleFor(x => x.SubjectType).MaximumLength(ActivityEntry.SubjectTypeMaxLength)
            .WithMessage("Тип об'єкта задовгий.");
        RuleFor(x => x.Action).Must(x => x is null || ActivityActions.All.Contains(x))
            .WithMessage("Невідома дія.");
        RuleFor(x => x.Search).MaximumLength(100).WithMessage("Пошук може мати до 100 символів.");
        RuleFor(x => x).Must(x => x.From is null || x.To is null || x.From <= x.To)
            .WithName("Період").WithMessage("Початок періоду має бути не пізніше кінця.");
    }
}
