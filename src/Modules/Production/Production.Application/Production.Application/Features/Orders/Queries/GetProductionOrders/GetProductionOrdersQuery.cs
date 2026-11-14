using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Queries.GetProductionOrders.Results;
using Production.Application.Features.Orders.Shared.Filtering;

namespace Production.Application.Features.Orders.Queries.GetProductionOrders;

/// <summary>«Виробничі завдання»: not deleted orders, newest number first unless <paramref name="Sort"/> says otherwise.</summary>
/// <param name="Status">New, Closed or Cancelled; null for all.</param>
/// <param name="DueTo">Only orders with an item due on or before this day.</param>
/// <param name="Stage">Only orders whose least advanced item is at this stage.</param>
/// <param name="CustomerId">The client card tab «Завдання в цех»: only orders made from this client's КП.</param>
/// <param name="QuoteId">The КП editor: only orders made from this КП.</param>
/// <param name="Filters">«Фільтри» of the list: client, order period, model, fabric, colour, season.</param>
/// <param name="ManagerUserId">Only orders of this manager (the КП responsible); «Робочий стіл менеджера» (DESK-001).</param>
/// <param name="Sort">One of <see cref="Sorts"/>: «№», «Готово до» (the nearest item due date, orders without one last)
/// or «Створено»; a leading «-» sorts from the largest. Null is «-number».</param>
public sealed record GetProductionOrdersQuery(
    string? Search, string? Status, string? Manager, DateOnly? DueTo, int Page = 1, int PageSize = 50,
    string? Stage = null, long? CustomerId = null, long? QuoteId = null, ProductionOrderFilters? Filters = null,
    string? Sort = null, int? ManagerUserId = null)
    : IQuery<Result<ProductionOrderPageResponse>>
{
    public const int MaxPageSize = 200;

    /// <summary>Days after today that still count as «скоро термін».</summary>
    public const int DueSoonDays = 3;

    public static readonly IReadOnlyList<string> Sorts = ["number", "-number", "due", "-due", "created", "-created"];
}
