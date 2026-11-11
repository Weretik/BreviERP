using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Domain.Orders;

namespace Production.Application.Orders.GetOrders;

public sealed class GetProductionOrdersQueryHandler(IProductionDbContext db, IProductionModelReader models)
    : IQueryHandler<GetProductionOrdersQuery, Result<ProductionOrderPageResponse>>
{
    public async ValueTask<Result<ProductionOrderPageResponse>> Handle(GetProductionOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var orders = await ProductionOrderFiltering.FilterAsync(db, models, query, cancellationToken);

        var byStatus = await orders.GroupBy(x => x.Status).Select(x => new { x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        int Count(ProductionOrderStatus status) => byStatus.FirstOrDefault(x => x.Key == status)?.Count ?? 0;
        var counts = new ProductionOrderCounts(byStatus.Sum(x => x.Count), Count(ProductionOrderStatus.New),
            Count(ProductionOrderStatus.Closed), Count(ProductionOrderStatus.Cancelled));

        orders = ProductionOrderFiltering.ByStatus(orders, query.Status);

        var total = await orders.CountAsync(cancellationToken);
        var rows = await orders
            .OrderByDescending(x => x.Number)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new ProductionOrderSummaryResponse(x.Id, x.Number, x.Status.ToString(), x.ClientName,
                x.QuoteId, x.QuoteNumber, x.ManagerName, x.OrderDate, x.Items.Count,
                x.Items.Sum(item => item.Quantity),
                x.Items.Min(item => (DateOnly?)item.DueDate),
                x.LegacyId != null, x.UpdatedAt,
                (x.Items.Min(item => (ProductionStage?)item.CurrentStage) ?? ProductionStage.New).ToString()))
            .ToListAsync(cancellationToken);
        var managers = await db.Orders.AsNoTracking().Where(x => x.DeletedAt == null)
            .Select(x => x.ManagerName).Distinct().OrderBy(x => x).ToListAsync(cancellationToken);
        return Result.Success(new ProductionOrderPageResponse(rows, query.Page, query.PageSize, total, counts, managers));
    }
}
