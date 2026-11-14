using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Work.Shared;

/// <summary>A record of «Журнал робіт».</summary>
/// <param name="Kind">Sewing or Cutting.</param>
/// <param name="Rate">The item's sewing rate in UAH when the work was recorded.</param>
public sealed record ProductionWorkResponse(
    int Id,
    int OrderId,
    int OrderNumber,
    string ClientName,
    int ItemId,
    string ModelName,
    int EmployeeId,
    string EmployeeName,
    string Kind,
    int Quantity,
    DateOnly WorkDate,
    bool OverPlan,
    string? Reason,
    string? Comment,
    decimal Rate,
    bool IsVoided,
    DateTimeOffset? VoidedAt,
    string? VoidedByName,
    string? VoidReason,
    string CreatedByName,
    DateTimeOffset CreatedAt);
