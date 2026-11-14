using Production.Application.Features.Orders.Shared;
namespace Production.Application.Features.Orders.Queries.GetProductionOrderFilterOptions;

/// <summary>Values «Фільтри» of «Виробничі завдання» offer, taken from the orders that are not deleted.</summary>
public sealed record ProductionOrderFilterOptionsResponse(
    IReadOnlyList<string> Clients,
    IReadOnlyList<ProductionFilterOption> Models,
    IReadOnlyList<ProductionFilterOption> Fabrics,
    IReadOnlyList<string> Colors,
    IReadOnlyList<string> Seasons);
