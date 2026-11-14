namespace Production.Application.Features.Orders.Queries.GetProductionModelSummary;

public sealed record ProductionModelSummaryResponse(IReadOnlyList<ProductionModelSummaryRow> Rows, int TotalQuantity);
