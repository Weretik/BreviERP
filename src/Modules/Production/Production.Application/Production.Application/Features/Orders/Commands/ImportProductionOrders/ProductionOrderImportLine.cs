using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Orders.Commands.ImportProductionOrders;

/// <param name="Action">created, unchanged or error.</param>
public sealed record ProductionOrderImportLine(int Number, string ClientName, int Items, string Action, string? Error);
