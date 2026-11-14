using Production.Application.Abstractions;
using Production.Domain.Archive;
using Production.Application.Features.Archive.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Archive.Queries.GetWorkshopArchive;

/// <summary>
/// «Архів цеху»: lines of one old book, newest first, found by any word and limited by date. Everyone with production
/// orders reads it (PRD-D54); there are no prices or rates in it.
/// </summary>
public sealed record GetWorkshopArchiveQuery(
    ProductionActor Actor, string Kind, string? Search, DateOnly? From, DateOnly? To, int Page, int PageSize)
    : IQuery<Result<WorkshopArchivePageResponse>>;

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

public sealed class GetWorkshopArchiveHandler(IProductionDbContext db)
    : IQueryHandler<GetWorkshopArchiveQuery, Result<WorkshopArchivePageResponse>>
{
    public async ValueTask<Result<WorkshopArchivePageResponse>> Handle(GetWorkshopArchiveQuery query,
        CancellationToken cancellationToken)
    {
        var kind = Enum.Parse<WorkshopArchiveKind>(query.Kind, ignoreCase: true);
        var rows = db.Archive.AsNoTracking().Where(x => x.Kind == kind);
        foreach (var word in (query.Search ?? string.Empty).ToLowerInvariant()
                     .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            rows = rows.Where(x => x.SearchText.Contains(word));
        if (query.From is { } from) rows = rows.Where(x => x.Date >= from);
        if (query.To is { } to) rows = rows.Where(x => x.Date <= to);

        var total = await rows.CountAsync(cancellationToken);
        var page = await rows.OrderByDescending(x => x.Date).ThenByDescending(x => x.SourceRow)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(cancellationToken);
        return Result.Success(new WorkshopArchivePageResponse(kind.ToString(), query.Page, query.PageSize, total,
            [.. page.Select(Map)]));
    }

    private static WorkshopArchiveRowResponse Map(WorkshopArchiveRecord x) => new(x.Id, x.SourceRow, x.Date,
        x.DateText, x.Number, x.Client, x.Product, x.Quantity, x.Fabric, x.Status, x.Notes,
        [.. x.Fields.Select(f => new WorkshopArchiveFieldResponse(f.Label, f.Value))],
        [.. x.Links.Select(l => new WorkshopArchiveLinkResponse(l.Label, l.Url))]);
}
