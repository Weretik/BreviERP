namespace Warehouse.Application.Features.Stock.Shared;

/// <summary>The pair after a stock operation.</summary>
/// <param name="Replayed">True when the same operation id was saved before and nothing moved now.</param>
public sealed record FabricStockOperationResponse(
    Guid? OperationId,
    string FabricCode,
    string Color,
    decimal Meters,
    bool Replayed);
