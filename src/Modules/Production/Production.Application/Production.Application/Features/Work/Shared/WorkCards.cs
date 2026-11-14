using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Work.Shared;

/// <summary>Builds «Робота» of an item and maps records; counts what the plan already holds.</summary>
public sealed class WorkCards(IProductionDbContext db)
{
    /// <summary>Pieces of the plan done by kind (records not cancelled and not over plan), per item.</summary>
    public async Task<Dictionary<int, (int Sewn, int Cut)>> DoneAsync(IReadOnlyCollection<int> itemIds,
        CancellationToken cancellationToken)
    {
        var rows = await db.Work.AsNoTracking()
            .Where(x => itemIds.Contains(x.ItemId) && x.VoidedAt == null && !x.OverPlan)
            .GroupBy(x => new { x.ItemId, x.Kind })
            .Select(g => new { g.Key.ItemId, g.Key.Kind, Quantity = g.Sum(x => x.Quantity) })
            .ToListAsync(cancellationToken);
        return itemIds.ToDictionary(id => id, id => (
            rows.Where(x => x.ItemId == id && x.Kind == ProductionWorkKind.Sewing).Sum(x => x.Quantity),
            rows.Where(x => x.ItemId == id && x.Kind == ProductionWorkKind.Cutting).Sum(x => x.Quantity)));
    }

    public async Task<ProductionItemWorkResponse> BuildAsync(ProductionOrder order, ProductionItem item,
        ProductionActor actor, CancellationToken cancellationToken)
    {
        var rows = await db.Work.AsNoTracking().Where(x => x.ItemId == item.Id)
            .OrderByDescending(x => x.WorkDate).ThenByDescending(x => x.Id).ToListAsync(cancellationToken);
        var counted = rows.Where(x => x.CountsToPlan).ToList();
        var canRecord = actor.CanRecordWork(order) && item.SewingRate is > 0;
        var cannot = !actor.CanRecordWork(order) ? (actor.SeesWork && !order.IsDeleted ? WorkMessages.NotNew : null)
            : item.SewingRate is > 0 ? null : WorkMessages.NoRate;
        return new ProductionItemWorkResponse(order.Id, item.Id, order.Number, item.ModelName, item.Quantity,
            item.SewingRate,
            counted.Where(x => x.Kind == ProductionWorkKind.Sewing).Sum(x => x.Quantity),
            counted.Where(x => x.Kind == ProductionWorkKind.Cutting).Sum(x => x.Quantity),
            [.. rows.Select(x => Map(x, order.Number, order.ClientName, item.ModelName))],
            canRecord, cannot);
    }

    public static ProductionWorkResponse Map(ProductionWork x, int orderNumber, string clientName, string modelName) =>
        new(x.Id, x.OrderId, orderNumber, clientName, x.ItemId, modelName, x.EmployeeId, x.EmployeeName,
            x.Kind.ToString(), x.Quantity, x.WorkDate, x.OverPlan, x.Reason, x.Comment, x.Rate, x.IsVoided, x.VoidedAt,
            x.VoidedByName, x.VoidReason, x.CreatedByName, x.CreatedAt);
}
