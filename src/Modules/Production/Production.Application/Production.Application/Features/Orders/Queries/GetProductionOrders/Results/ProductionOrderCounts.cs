namespace Production.Application.Features.Orders.Queries.GetProductionOrders.Results;

/// <summary>Counts by status for the filter chips; they ignore the status filter itself.</summary>
public sealed record ProductionOrderCounts(int All, int New, int Closed, int Cancelled);
