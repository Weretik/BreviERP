namespace Production.Application.Features.Orders.Commands.CancelProductionOrder;

public sealed record CancelProductionOrderInput(int Version, string Reason);
