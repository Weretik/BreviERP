namespace Production.Application.Abstractions.Materials;

/// <summary>
/// «Склад тканин» (Warehouse) for «Звіти цеху»: fabric issued to the workshop and the stock balance, by operation date.
/// Implemented in the host.
/// </summary>
public interface IProductionStockReader
{
    /// <summary>Issues not taken back with operation date in [<paramref name="from"/>, <paramref name="to"/>), oldest first.</summary>
    Task<IReadOnlyList<ProductionStockIssue>> IssuesAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);

    /// <summary>Every stock line and its balance before <paramref name="before"/>.</summary>
    Task<IReadOnlyList<ProductionStockBalance>> BalancesAsync(DateOnly before, CancellationToken cancellationToken);
}

public sealed record ProductionStockIssue(int MovementId, DateOnly OperationDate, string FabricCode, string FabricName,
    string Color, int? OrderId, int? OrderNumber, string? ModelName, decimal Meters);

public sealed record ProductionStockBalance(int StockId, string FabricCode, string FabricName, string Color,
    decimal Meters);
