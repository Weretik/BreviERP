using Production.Application.Abstractions;
using Production.Domain.Archive;
using Production.Application.Features.Archive.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Archive.Queries.GetWorkshopArchive;

public sealed class GetWorkshopArchiveValidator : AbstractValidator<GetWorkshopArchiveQuery>
{
    public GetWorkshopArchiveValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Kind).Must(x => Enum.TryParse<WorkshopArchiveKind>(x, true, out var kind) && Enum.IsDefined(kind))
            .WithMessage(ArchiveMessages.Kind);
        RuleFor(x => x.Search).MaximumLength(200);
        RuleFor(x => x.From).LessThanOrEqualTo(x => x.To).When(x => x.From is not null && x.To is not null)
            .WithMessage(ArchiveMessages.Period);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, ArchiveMessages.MaxPageSize);
    }
}
