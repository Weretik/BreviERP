namespace Production.Application.Features.Orders.Shared.Responses;

public sealed record ProductionShipmentResponse(
    int Id, int Quantity, DateOnly ShippedOn, string? Comment, string CreatedByName, DateTimeOffset CreatedAt);
