using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Facts;

/// <summary>A «Видача» movement that was not taken back: what went to the workshop and when.</summary>
public sealed record StockIssueFact(int MovementId, DateOnly OperationDate, string FabricCode, string FabricName,
    string Color, int? OrderId, int? OrderNumber, string? ModelName, decimal Meters);

/// <summary>A stock line and its balance at a moment.</summary>
public sealed record StockBalanceFact(int StockId, string FabricCode, string FabricName, string Color, decimal Meters);

/// <summary>
/// Facts of the stock for other modules («Звіти цеху» of Production): issues over a period and balances at a date,
/// both by operation date. The host passes them on through Production's port.
/// </summary>
public sealed class StockFacts(IWarehouseDbContext db)
{
    /// <summary>Issues with operation date in [<paramref name="from"/>, <paramref name="to"/>), oldest first.</summary>
    public async Task<IReadOnlyList<StockIssueFact>> IssuesAsync(DateOnly from, DateOnly to,
        CancellationToken cancellationToken) =>
        (await db.FabricMovements.AsNoTracking().Include(x => x.Stock)
            .Where(x => x.Kind == FabricMovementKind.Issue && !x.IsReversed &&
                        x.OperationDate >= from && x.OperationDate < to)
            .OrderBy(x => x.OperationDate).ThenBy(x => x.Id)
            .ToListAsync(cancellationToken))
        .Select(x => new StockIssueFact(x.Id, x.OperationDate, x.Stock.FabricCode, x.Stock.FabricName, x.Stock.Color,
            x.OrderId, x.OrderNumber, x.ModelName, -x.Delta))
        .ToList();

    /// <summary>Every line with its balance before <paramref name="before"/>: later movements are taken off.</summary>
    public async Task<IReadOnlyList<StockBalanceFact>> BalancesAsync(DateOnly before,
        CancellationToken cancellationToken)
    {
        var stocks = await db.FabricStocks.AsNoTracking().ToListAsync(cancellationToken);
        var later = (await db.FabricMovements.AsNoTracking().Where(x => x.OperationDate >= before)
                .Select(x => new { x.StockId, x.Delta }).ToListAsync(cancellationToken))
            .GroupBy(x => x.StockId).ToDictionary(x => x.Key, x => x.Sum(m => m.Delta));
        return stocks.Select(x => new StockBalanceFact(x.Id, x.FabricCode, x.FabricName, x.Color,
                x.Meters - later.GetValueOrDefault(x.Id)))
            .ToList();
    }
}
