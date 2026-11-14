namespace Warehouse.Contracts.Stock;

/// <summary>
/// Facts of «Склад тканин» for other modules («Звіти цеху» of Production): issues over a period and balances at a
/// date, both by operation date.
/// </summary>
public interface IStockFacts
{
    /// <summary>Issues with operation date in [<paramref name="from"/>, <paramref name="to"/>), oldest first.</summary>
    Task<IReadOnlyList<StockIssueFact>> IssuesAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);

    /// <summary>Every line with its balance before <paramref name="before"/>: later movements are taken off.</summary>
    Task<IReadOnlyList<StockBalanceFact>> BalancesAsync(DateOnly before, CancellationToken cancellationToken);
}

/// <summary>A «Видача» movement that was not taken back: what went to the workshop and when.</summary>
public sealed record StockIssueFact(int MovementId, DateOnly OperationDate, string FabricCode, string FabricName,
    string Color, int? OrderId, int? OrderNumber, string? ModelName, decimal Meters);

/// <summary>A stock line and its balance at a moment.</summary>
public sealed record StockBalanceFact(int StockId, string FabricCode, string FabricName, string Color, decimal Meters);
