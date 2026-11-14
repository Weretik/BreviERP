using Production.Application.Abstractions;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Queries.GetWorkLog;

/// <summary>«Журнал робіт» of a month «РРРР-ММ», optionally of one worker or one order.</summary>
public sealed record GetWorkLogQuery(ProductionActor Actor, string Month, int? EmployeeId, int? OrderId)
    : IQuery<Result<WorkLogResponse>>;

public sealed class GetWorkLogValidator : AbstractValidator<GetWorkLogQuery>
{
    public GetWorkLogValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => WorkMonth.TryParse(x, out _)).WithMessage(WorkMessages.Month);
        RuleFor(x => x.EmployeeId).GreaterThan(0).When(x => x.EmployeeId is not null);
        RuleFor(x => x.OrderId).GreaterThan(0).When(x => x.OrderId is not null);
    }
}

public sealed class GetWorkLogHandler(IProductionDbContext db) : IQueryHandler<GetWorkLogQuery, Result<WorkLogResponse>>
{
    public async ValueTask<Result<WorkLogResponse>> Handle(GetWorkLogQuery query, CancellationToken cancellationToken)
    {
        if (!query.Actor.SeesWork) return Result<WorkLogResponse>.Forbidden(WorkMessages.CannotSee);
        WorkMonth.TryParse(query.Month, out var first);
        var next = first.AddMonths(1);
        var work = db.Work.AsNoTracking().Where(x => x.WorkDate >= first && x.WorkDate < next);
        if (query.EmployeeId is { } employee) work = work.Where(x => x.EmployeeId == employee);
        if (query.OrderId is { } order) work = work.Where(x => x.OrderId == order);
        var rows = await work.OrderByDescending(x => x.WorkDate).ThenByDescending(x => x.Id).ToListAsync(cancellationToken);
        var orderIds = rows.Select(x => x.OrderId).Distinct().ToList();
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items).Where(x => orderIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var mapped = rows.Select(x =>
        {
            var o = orders[x.OrderId];
            return WorkCards.Map(x, o.Number, o.ClientName, o.Items.First(i => i.Id == x.ItemId).ModelName);
        }).ToList();
        var counted = rows.Where(x => !x.IsVoided).ToList();
        return Result.Success(new WorkLogResponse(query.Month, mapped, new WorkLogTotals(
            counted.Where(x => x.Kind == ProductionWorkKind.Sewing).Sum(x => x.Quantity),
            counted.Where(x => x.Kind == ProductionWorkKind.Cutting).Sum(x => x.Quantity),
            counted.Count)));
    }
}

public static class WorkMonth
{
    public static bool TryParse(string? value, out DateOnly first)
    {
        first = default;
        return value is { Length: 7 } && DateOnly.TryParseExact($"{value}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out first);
    }
}
