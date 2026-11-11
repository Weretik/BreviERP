using Production.Application.Abstractions;
using Warehouse.Application.Stock.Facts;

namespace Host.Api.Production;

/// <summary>«Склад тканин» (Warehouse) for «Звіти цеху»: issued fabric and the stock balance at a date.</summary>
internal sealed class ProductionStockReader(StockFacts facts) : IProductionStockReader
{
    public async Task<IReadOnlyList<ProductionStockIssue>> IssuesAsync(DateOnly from, DateOnly to,
        CancellationToken cancellationToken) =>
        (await facts.IssuesAsync(from, to, cancellationToken))
        .Select(x => new ProductionStockIssue(x.MovementId, x.OperationDate, x.FabricCode, x.FabricName, x.Color,
            x.OrderId, x.OrderNumber, x.ModelName, x.Meters))
        .ToArray();

    public async Task<IReadOnlyList<ProductionStockBalance>> BalancesAsync(DateOnly before,
        CancellationToken cancellationToken) =>
        (await facts.BalancesAsync(before, cancellationToken))
        .Select(x => new ProductionStockBalance(x.StockId, x.FabricCode, x.FabricName, x.Color, x.Meters))
        .ToArray();
}
