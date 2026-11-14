using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Queries.GetWorkChoices;

/// <summary>Workers who work and items of «Нове» orders, for «Додати роботу» in «Журнал робіт».</summary>
public sealed record GetWorkChoicesQuery(ProductionActor Actor) : IQuery<Result<WorkChoicesResponse>>;

public sealed class GetWorkChoicesValidator : AbstractValidator<GetWorkChoicesQuery>
{
    public GetWorkChoicesValidator() => RuleFor(x => x.Actor).ValidActor();
}

public sealed class GetWorkChoicesHandler(IProductionDbContext db, WorkCards cards)
    : IQueryHandler<GetWorkChoicesQuery, Result<WorkChoicesResponse>>
{
    public async ValueTask<Result<WorkChoicesResponse>> Handle(GetWorkChoicesQuery query, CancellationToken cancellationToken)
    {
        if (!query.Actor.SeesWork) return Result<WorkChoicesResponse>.Forbidden(WorkMessages.CannotSee);
        var employees = await db.Employees.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.FullName)
            .Select(x => new WorkEmployeeChoice(x.Id, x.FullName, x.Position)).ToListAsync(cancellationToken);
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items)
            .Where(x => x.DeletedAt == null && x.Status == ProductionOrderStatus.New)
            .OrderBy(x => x.Number).ToListAsync(cancellationToken);
        var done = await cards.DoneAsync([.. orders.SelectMany(x => x.Items).Select(x => x.Id)], cancellationToken);
        var items = orders.SelectMany(o => o.Items.OrderBy(x => x.Position).Select(i => new WorkItemChoice(o.Id, o.Number,
            o.ClientName, i.Id, i.ModelName, i.Quantity, done[i.Id].Sewn, done[i.Id].Cut, i.SewingRate))).ToList();
        return Result.Success(new WorkChoicesResponse(employees, items, OverPlanReasons.All));
    }
}
