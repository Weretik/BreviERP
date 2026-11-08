using Production.Application.Contracts;

namespace Production.Application.Orders.GetOrders;

/// <summary>«Виробничі завдання»: not deleted orders, newest number first.</summary>
/// <param name="Status">New, Closed or Cancelled; null for all.</param>
/// <param name="DueTo">Only orders with an item due on or before this day.</param>
/// <param name="Stage">Only orders whose least advanced item is at this stage.</param>
/// <param name="CustomerId">The client card tab «Завдання в цех»: only orders made from this client's КП.</param>
/// <param name="QuoteId">The КП editor: only orders made from this КП.</param>
public sealed record GetProductionOrdersQuery(
    string? Search, string? Status, string? Manager, DateOnly? DueTo, int Page = 1, int PageSize = 50,
    string? Stage = null, long? CustomerId = null, long? QuoteId = null)
    : IQuery<Result<ProductionOrderPageResponse>>
{
    public const int MaxPageSize = 200;
}
