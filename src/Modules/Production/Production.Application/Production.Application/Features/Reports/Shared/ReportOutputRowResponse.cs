using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Reports.Shared;

/// <summary>Pieces sewn of an item in the months (within the plan, not cancelled) at the item's КП price.</summary>
public sealed record ReportOutputRowResponse(int OrderId, int OrderNumber, string ClientName, int ItemId,
    string ModelName, int Quantity, decimal? UnitPrice, decimal? Value);
