namespace Production.Application.Features.Orders.Queries.GetProductionOrders.Results;

/// <summary>An item of a list row: the model, its pieces and its current stage, in the order of the card.</summary>
public sealed record ProductionOrderSummaryItem(string ModelName, int Quantity, string Stage);
