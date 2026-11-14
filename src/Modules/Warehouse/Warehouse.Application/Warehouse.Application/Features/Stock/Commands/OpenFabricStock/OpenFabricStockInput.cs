namespace Warehouse.Application.Features.Stock.Commands.OpenFabricStock;

/// <summary>«Початковий залишок»: once per fabric and colour, zero allowed.</summary>
/// <param name="OperationId">A new id per form; the same id repeats the saved result.</param>
public sealed record OpenFabricStockInput(
    Guid OperationId,
    int FabricId,
    string Color,
    decimal Meters,
    DateOnly OperationDate);
