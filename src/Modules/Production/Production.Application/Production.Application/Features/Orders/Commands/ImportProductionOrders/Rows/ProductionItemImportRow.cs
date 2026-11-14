using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Orders.Commands.ImportProductionOrders.Rows;

public sealed record ProductionItemImportRow(
    string? ModelCode,
    string ModelName,
    string? FabricCode,
    string? FabricName,
    decimal? SewingRate,
    ProductionItemDetailsInput Details);
