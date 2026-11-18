using Warehouse.Application.Abstractions;
using Production.Contracts.FabricPlans;
using Warehouse.Domain.Stock;
using Warehouse.Domain.Stock.Movements;

namespace Warehouse.Application.Features.Stock.Shared.Facts;

/// <summary>
/// Facts of the stock for «Звіти цеху» of Production (its <see cref="IStockFacts"/>, ADR-0019): issues over a period
/// and balances at a date, both by operation date.
/// </summary>
public sealed class StockFacts(IWarehouseDbContext db) : IStockFacts
{
    public async Task<IReadOnlyList<StockIssueFact>> IssuesAsync(DateOnly from, DateOnly to,
        CancellationToken cancellationToken) =>
        (await db.FabricMovements.AsNoTracking().Include(x => x.Stock)
            .Where(x => x.Kind == FabricMovementKind.Issue && !x.IsReversed &&
                        x.OperationDate >= from && x.OperationDate < to)
            .OrderBy(x => x.OperationDate).ThenBy(x => x.Id)
            .ToListAsync(cancellationToken))
        .Select(x => new StockIssueFact(x.Id.Value, x.OperationDate, x.Stock.FabricCode, x.Stock.FabricName, x.Stock.Color,
            x.OrderId?.Value, x.OrderNumber, x.ModelName, -x.Delta))
        .ToList();

    public async Task<IReadOnlyList<StockBalanceFact>> BalancesAsync(DateOnly before,
        CancellationToken cancellationToken)
    {
        var stocks = await db.FabricStocks.AsNoTracking().ToListAsync(cancellationToken);
        var later = (await db.FabricMovements.AsNoTracking().Where(x => x.OperationDate >= before)
                .Select(x => new { x.StockId, x.Delta }).ToListAsync(cancellationToken))
            .GroupBy(x => x.StockId).ToDictionary(x => x.Key, x => x.Sum(m => m.Delta));
        return stocks.Select(x => new StockBalanceFact(x.Id.Value, x.FabricCode, x.FabricName, x.Color,
                x.Meters - later.GetValueOrDefault(x.Id)))
            .ToList();
    }
}
