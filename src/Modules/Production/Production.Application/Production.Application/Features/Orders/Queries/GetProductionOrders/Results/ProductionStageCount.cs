namespace Production.Application.Features.Orders.Queries.GetProductionOrders.Results;

/// <summary>A stage of the «конвеєр»: «Нове» orders whose least advanced item is at it, and how many are overdue.</summary>
public sealed record ProductionStageCount(string Stage, int Count, int Overdue);
