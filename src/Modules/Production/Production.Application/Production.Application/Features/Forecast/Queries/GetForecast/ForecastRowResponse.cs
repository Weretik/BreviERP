using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Forecast.Queries.GetForecast;

/// <summary>An order of «Прогноз пошиття».</summary>
/// <param name="Minutes">Sewing minutes left; only for people who see the sewing rate.</param>
/// <param name="FinishOn">Approximate end of sewing; null when it cannot be counted.</param>
/// <param name="Status">Estimated, Complete or Unknown.</param>
/// <param name="Reason">MissingRate, UnknownPredecessor or BeyondHorizon when there is no date.</param>
/// <param name="DueDate">The earliest «Готово до» of the order's items.</param>
public sealed record ForecastRowResponse(
    int OrderId,
    int OrderNumber,
    string ClientName,
    string ManagerName,
    int Remaining,
    decimal? Minutes,
    DateOnly? FinishOn,
    string Status,
    string? Reason,
    DateOnly DueDate,
    bool IsLate);
