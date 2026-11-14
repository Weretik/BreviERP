namespace Warehouse.Application.Features.Stock.Commands.ReceiveFabric;

/// <summary>«Прихід» of the offer picked.</summary>
public sealed record ReceiveFabricInput(
    Guid OperationId,
    int FabricId,
    string Color,
    decimal Meters,
    DateOnly OperationDate,
    string? SupplierName,
    string? Document,
    string? Note);
