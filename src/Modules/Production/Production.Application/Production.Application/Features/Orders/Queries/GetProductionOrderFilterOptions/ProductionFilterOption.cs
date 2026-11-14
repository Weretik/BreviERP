namespace Production.Application.Features.Orders.Queries.GetProductionOrderFilterOptions;

/// <summary>A model or fabric of an order item: its code and name.</summary>
public sealed record ProductionFilterOption(string Code, string Name);
