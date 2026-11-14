namespace Production.Application.Features.Orders.Commands.CreateProductionShipment;

public sealed record CreateProductionShipmentInput(int Version, int Quantity, DateOnly ShippedOn, string? Comment);
