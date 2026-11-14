namespace Warehouse.Application.Features.Stock.Commands.IssueFabric;

/// <summary>«Разова витрата / без замовлення» of a pair; the balance may go below zero.</summary>
/// <param name="ExpectedVersion">The pair version shown on the form; null skips the check.</param>
public sealed record IssueFabricInput(
    Guid OperationId,
    string FabricCode,
    string Color,
    string? ExpectedVersion,
    decimal Meters,
    DateOnly OperationDate,
    string? Destination,
    string? Note);
