using BuildingBlocks.Domain.Identifiers.Orders;
using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;
using Production.Domain.Orders.Enums;
using Production.Application.Abstractions.Sales;
using Production.Application.Features.Orders.Queries.GetProductionOrders.Results;
using Production.Application.Features.Orders.Shared.Filtering;

namespace Production.Application.Features.Orders.Queries.GetProductionOrders;

public sealed class GetProductionOrdersHandler(IProductionDbContext db, IProductionModelReader models,
    IProductionCustomerReader customers, TimeProvider clock)
    : IQueryHandler<GetProductionOrdersQuery, Result<ProductionOrderPageResponse>>
{
    public async ValueTask<Result<ProductionOrderPageResponse>> Handle(GetProductionOrdersQuery query,
        CancellationToken cancellationToken)
    {
        if (query.CustomerPublicId is { } publicId)
        {
            // Ids start at 1, so an unknown client (0) matches no order.
            var customerId = await customers.FindInternalIdAsync(publicId, cancellationToken);
            query = query with { CustomerId = customerId ?? 0 };
        }

        var orders = await ProductionOrderFiltering.FilterAsync(db, models, query, cancellationToken);

        var byStatus = await orders.GroupBy(x => x.Status).Select(x => new { x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        int Count(ProductionOrderStatus status) => byStatus.FirstOrDefault(x => x.Key == status)?.Count ?? 0;
        var counts = new ProductionOrderCounts(byStatus.Sum(x => x.Count), Count(ProductionOrderStatus.New),
            Count(ProductionOrderStatus.Closed), Count(ProductionOrderStatus.Cancelled));

        var (stages, attention) = await ConveyorAsync(query, cancellationToken);

        orders = ProductionOrderFiltering.ByStatus(orders, query.Status);

        var total = await orders.CountAsync(cancellationToken);
        var totalQuantity = await orders.SumAsync(x => x.Items.Sum(item => item.Quantity), cancellationToken);
        var rows = await Sorted(orders, query.Sort)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new ProductionOrderSummaryResponse(x.Id.Value, x.Number, x.Status.ToString(), x.ClientName,
                x.QuoteId != null ? x.QuoteId.Value.Value : (long?)null, x.QuoteNumber, x.ManagerName, x.OrderDate, x.Items.Count,
                x.Items.Sum(item => item.Quantity),
                x.Items.Min(item => (DateOnly?)item.DueDate),
                x.LegacyId != null, x.UpdatedAt,
                (x.Items.Min(item => (ProductionStage?)item.CurrentStage) ?? ProductionStage.New).ToString(),
                x.Shipments.Sum(shipment => shipment.Quantity),
                x.Items.OrderBy(item => item.Position)
                    .Select(item => new ProductionOrderSummaryItem(item.ModelCode, item.ModelName, item.Quantity,
                        item.CurrentStage.ToString(), item.FabricName, item.MainColor, new List<string>(),
                        new List<string>()))
                    .ToList()))
            .ToListAsync(cancellationToken);
        rows = await WithDetailsAsync(rows, cancellationToken);
        var managers = await db.Orders.AsNoTracking().Where(x => x.DeletedAt == null)
            .Select(x => x.ManagerName).Distinct().OrderBy(x => x).ToListAsync(cancellationToken);
        return Result.Success(new ProductionOrderPageResponse(rows, query.Page, query.PageSize, total, counts, managers,
            stages, attention, totalQuantity));
    }

    /// <summary>
    /// Adds the additional colours and the kinds of «нанесення» of the page's items; they are stored as JSON, so the
    /// few orders of the page are read as entities.
    /// </summary>
    private async Task<List<ProductionOrderSummaryResponse>> WithDetailsAsync(
        List<ProductionOrderSummaryResponse> rows, CancellationToken cancellationToken)
    {
        var ids = rows.Select(x => new ProductionOrderId(x.Id)).ToList();
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items).Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
        var details = orders.ToDictionary(x => x.Id.Value,
            x => x.Items.OrderBy(item => item.Position).ToList());
        return [.. rows.Select(row => details.TryGetValue(row.Id, out var items) && items.Count == row.Items.Count
            ? row with
            {
                Items = [.. row.Items.Zip(items, (summary, item) => summary with
                {
                    AdditionalColors = item.AdditionalColors,
                    Applications = [.. item.Applications.Select(x => x.Kind.ToString()).Distinct()]
                })]
            }
            : row)];
    }

    private static IQueryable<ProductionOrder> Sorted(IQueryable<ProductionOrder> orders, string? sort) => sort switch
    {
        "number" => orders.OrderBy(x => x.Number),
        "due" => orders
            .OrderBy(x => x.Items.Min(item => (DateOnly?)item.DueDate) == null)
            .ThenBy(x => x.Items.Min(item => (DateOnly?)item.DueDate))
            .ThenBy(x => x.Number),
        "-due" => orders
            .OrderBy(x => x.Items.Min(item => (DateOnly?)item.DueDate) == null)
            .ThenByDescending(x => x.Items.Min(item => (DateOnly?)item.DueDate))
            .ThenByDescending(x => x.Number),
        "created" => orders.OrderBy(x => x.OrderDate).ThenBy(x => x.Number),
        "-created" => orders.OrderByDescending(x => x.OrderDate).ThenByDescending(x => x.Number),
        _ => orders.OrderByDescending(x => x.Number),
    };

    /// <summary>
    /// The «конвеєр» over «Нове» orders with every filter but status and stage: how many stand at each stage, how
    /// many of them are overdue, and how many are due soon. There are few open orders, so they are counted in memory.
    /// </summary>
    private async Task<(IReadOnlyList<ProductionStageCount>, ProductionOrderAttention)> ConveyorAsync(
        GetProductionOrdersQuery query, CancellationToken cancellationToken)
    {
        var open = (await ProductionOrderFiltering.FilterAsync(db, models, query, cancellationToken, withStage: false))
            .Where(x => x.Status == ProductionOrderStatus.New);
        var rows = await open
            .Select(x => new
            {
                Stage = x.Items.Min(item => (ProductionStage?)item.CurrentStage) ?? ProductionStage.New,
                Due = x.Items.Min(item => (DateOnly?)item.DueDate),
            })
            .ToListAsync(cancellationToken);

        var today = WorkshopDay.Today(clock);
        var soon = today.AddDays(GetProductionOrdersQuery.DueSoonDays);
        bool Overdue(DateOnly? due) => due < today;

        var stages = Enum.GetValues<ProductionStage>()
            .Select(stage => new ProductionStageCount(stage.ToString(),
                rows.Count(x => x.Stage == stage), rows.Count(x => x.Stage == stage && Overdue(x.Due))))
            .ToList();
        var attention = new ProductionOrderAttention(rows.Count(x => Overdue(x.Due)),
            rows.Count(x => x.Due >= today && x.Due <= soon));
        return (stages, attention);
    }
}
