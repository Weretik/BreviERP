namespace Production.Application.Features.Orders.Queries.GetProductionOrders;

/// <param name="Stages">Every stage in workshop order; counts ignore the status and stage filters and take «Нове» orders only.</param>
/// <param name="Attention">Over the same orders as <paramref name="Stages"/>.</param>
public sealed record ProductionOrderPageResponse(
    IReadOnlyList<ProductionOrderSummaryResponse> Items, int Page, int PageSize, int Total,
    ProductionOrderCounts Counts, IReadOnlyList<string> Managers,
    IReadOnlyList<ProductionStageCount> Stages, ProductionOrderAttention Attention);
