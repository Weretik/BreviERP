using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Domain.Orders;

namespace Production.Application.Orders.GetOrders;

public sealed class GetProductionOrdersQueryHandler(IProductionDbContext db)
    : IQueryHandler<GetProductionOrdersQuery, Result<ProductionOrderPageResponse>>
{
    public async ValueTask<Result<ProductionOrderPageResponse>> Handle(GetProductionOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var orders = db.Orders.AsNoTracking().Where(x => x.DeletedAt == null);
        if (query.CustomerId is { } customerId)
            orders = orders.Where(x => x.CustomerId == customerId);
        if (query.QuoteId is { } quoteId)
            orders = orders.Where(x => x.QuoteId == quoteId);

        if (!string.IsNullOrWhiteSpace(query.Manager))
        {
            var manager = query.Manager.Trim();
            orders = orders.Where(x => x.ManagerName == manager);
        }

        if (query.Stage is not null)
        {
            var stage = Enum.Parse<ProductionStage>(query.Stage, ignoreCase: true);
            orders = orders.Where(x => x.Items.Min(item => item.CurrentStage) == stage);
        }

        if (query.DueTo is { } dueTo)
            orders = orders.Where(x => x.Items.Any(item => item.DueDate <= dueTo));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            var number = int.TryParse(search.TrimStart('№', ' '), NumberStyles.None, CultureInfo.InvariantCulture, out var n)
                ? n
                : (int?)null;
            orders = orders.Where(x => x.Number == number ||
                x.ClientName.ToLower().Contains(search) ||
                (x.QuoteNumber != null && x.QuoteNumber.ToLower().Contains(search)) ||
                x.Items.Any(item => item.ModelName.ToLower().Contains(search) ||
                    (item.ModelCode != null && item.ModelCode.ToLower().Contains(search))));
        }

        var byStatus = await orders.GroupBy(x => x.Status).Select(x => new { x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        int Count(ProductionOrderStatus status) => byStatus.FirstOrDefault(x => x.Key == status)?.Count ?? 0;
        var counts = new ProductionOrderCounts(byStatus.Sum(x => x.Count), Count(ProductionOrderStatus.New),
            Count(ProductionOrderStatus.Closed), Count(ProductionOrderStatus.Cancelled));

        if (query.Status is not null)
        {
            var status = Enum.Parse<ProductionOrderStatus>(query.Status, ignoreCase: true);
            orders = orders.Where(x => x.Status == status);
        }

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
