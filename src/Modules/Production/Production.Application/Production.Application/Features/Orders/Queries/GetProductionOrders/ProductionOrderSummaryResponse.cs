using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Orders.Queries.GetProductionOrders;

public sealed record ProductionOrderSummaryResponse(
    int Id,
    int Number,
    string Status,
    string ClientName,
    long? QuoteId,
    string? QuoteNumber,
    string ManagerName,
    DateOnly OrderDate,
    int ItemsCount,
    int TotalQuantity,
    DateOnly? NearestDueDate,
    bool IsTransferred,
    DateTimeOffset UpdatedAt,
    string Stage,
    int ShippedQuantity,
    IReadOnlyList<ProductionOrderSummaryItem> Items);
