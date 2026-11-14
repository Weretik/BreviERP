using Production.Application.Features.Orders.Shared;
namespace Production.Application.Features.Orders.Commands.ImportProductionOrders;

public sealed record ProductionItemImportRow(
    string? ModelCode,
    string ModelName,
    string? FabricCode,
    string? FabricName,
    decimal? SewingRate,
    ProductionItemDetailsInput Details);
