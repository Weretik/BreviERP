namespace Warehouse.Application.Features.Stock.Commands.AdjustFabricStock;

/// <summary>«Уточнити»: the counted total of a pair with a reason.</summary>
public sealed record AdjustFabricStockInput(
    Guid OperationId,
    string FabricCode,
    string Color,
    string ExpectedVersion,
    decimal TargetMeters,
    DateOnly OperationDate,
    string Reason);
