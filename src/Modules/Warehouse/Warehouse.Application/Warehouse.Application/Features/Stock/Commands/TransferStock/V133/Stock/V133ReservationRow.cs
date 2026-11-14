namespace Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Stock;

/// <summary><c>fabric_reservations</c>: the saved main-fabric plan of an item.</summary>
public sealed record V133ReservationRow(
    string Id,
    string? OrderId,
    string ItemId,
    string FabricId,
    string Color,
    long HundredthsOfMeter);
