namespace Production.Application.Features.Orders.Queries.GetProductionOrders.Results;

/// <summary>«Нове» orders that need attention: due before today, or due today or within the next three days.</summary>
public sealed record ProductionOrderAttention(int Overdue, int DueSoon);
