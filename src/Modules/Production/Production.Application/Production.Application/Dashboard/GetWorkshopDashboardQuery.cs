using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Application.Work;
using Production.Domain.Orders;

namespace Production.Application.Dashboard;

/// <summary>
/// «Огляд виробництва» on the admin home (PRD-D50): the «Нове» orders on a board by the stage of their slowest item,
/// how many are late and what is ready to ship. Everyone with production orders sees it; no prices or rates.
/// </summary>
public sealed record GetWorkshopDashboardQuery(ProductionActor Actor) : IQuery<Result<WorkshopDashboardResponse>>;

public sealed class GetWorkshopDashboardQueryValidator : AbstractValidator<GetWorkshopDashboardQuery>
{
    public GetWorkshopDashboardQueryValidator() => RuleFor(x => x.Actor).ValidActor();
}

public sealed class GetWorkshopDashboardQueryHandler(IProductionDbContext db, WorkCards cards, TimeProvider clock)
    : IQueryHandler<GetWorkshopDashboardQuery, Result<WorkshopDashboardResponse>>
{
    /// <summary>The columns of the board: from «Нове» to «Готово до відвантаження».</summary>
    private static readonly ProductionStage[] Columns =
    [
        ProductionStage.New, ProductionStage.FabricOrdered, ProductionStage.FabricAvailable, ProductionStage.Cutting,
        ProductionStage.Application, ProductionStage.Sewing, ProductionStage.Ready
    ];

    public async ValueTask<Result<WorkshopDashboardResponse>> Handle(GetWorkshopDashboardQuery query,
        CancellationToken cancellationToken)
    {
        var today = WorkshopDay.Today(clock);
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items).Include(x => x.Shipments)
            .Where(x => x.DeletedAt == null && x.Status == ProductionOrderStatus.New)
            .OrderBy(x => x.Number)
            .ToListAsync(cancellationToken);
        var done = await cards.DoneAsync([.. orders.SelectMany(x => x.Items).Select(x => x.Id)], cancellationToken);

        var cardsByStage = new Dictionary<ProductionStage, List<DashboardOrderResponse>>();
        int late = 0, ready = 0, readyPieces = 0;
        foreach (var order in orders)
        {
            var shipped = order.Shipments.GroupBy(x => x.ItemId).ToDictionary(x => x.Key, x => x.Sum(s => s.Quantity));
            var open = order.Items.Where(x => shipped.GetValueOrDefault(x.Id) < x.Quantity).ToList();
            var waiting = open.Where(x => x.CurrentStage < ProductionStage.Shipped).ToList();
            var toShip = waiting.Where(x => x.CurrentStage == ProductionStage.Ready)
                .Sum(x => x.Quantity - shipped.GetValueOrDefault(x.Id));
            if (toShip > 0)
            {
                ready++;
                readyPieces += toShip;
            }

            var dueDate = open.Count > 0 ? open.Min(x => x.DueDate) : order.Items.Select(x => x.DueDate).DefaultIfEmpty(order.OrderDate).Min();
            var isLate = open.Count > 0 && dueDate < today;
            if (isLate) late++;
            if (waiting.Count == 0) continue;

            var stage = waiting.Min(x => x.CurrentStage);
            var quantity = order.Items.Sum(x => x.Quantity);
            var sewn = order.Items.Sum(x => Math.Min(x.Quantity, done.GetValueOrDefault(x.Id).Sewn));
            var first = order.Items.OrderBy(x => x.Position).First();
            if (!cardsByStage.TryGetValue(stage, out var list)) cardsByStage[stage] = list = [];
            list.Add(new DashboardOrderResponse(order.Id, order.Number, order.ClientName, first.ModelName,
                order.Items.Count, quantity, quantity > 0 ? sewn * 100 / quantity : 0, dueDate, isLate));
        }

        var stages = Columns.Select(stage => new DashboardStageResponse((int)stage, ProductionStages.Label(stage),
            [.. cardsByStage.GetValueOrDefault(stage, []).OrderBy(x => x.DueDate).ThenBy(x => x.Number)])).ToList();
        var hours = query.Actor.CanKeepTime ? await HoursAsync(today, cancellationToken) : null;
        return Result.Success(new WorkshopDashboardResponse(today, orders.Count,
            orders.Sum(x => x.Items.Sum(i => i.Quantity)), late, ready, readyPieces, stages, hours));
    }

    /// <summary>«Робочий час» of this week, Monday to today, by active worker.</summary>
    private async Task<IReadOnlyList<DashboardHoursResponse>> HoursAsync(DateOnly today,
        CancellationToken cancellationToken)
    {
        var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var minutes = (await db.TimeEntries.AsNoTracking()
                .Where(x => x.WorkDate >= monday && x.WorkDate <= today)
                .Select(x => new { x.EmployeeId, x.Minutes }).ToListAsync(cancellationToken))
            .GroupBy(x => x.EmployeeId).ToDictionary(x => x.Key, x => x.Sum(m => m.Minutes));
        var employees = await db.Employees.AsNoTracking().Where(x => x.IsActive).ToListAsync(cancellationToken);
        return [.. employees
            .Select(x => new DashboardHoursResponse(x.Id, x.FullName, minutes.GetValueOrDefault(x.Id)))
            .OrderByDescending(x => x.Minutes).ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)];
    }
}
