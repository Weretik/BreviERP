namespace Production.Application.Contracts;

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

/// <param name="MinuteCost">Only for Admin.</param>
public sealed record ForecastSettingsResponse(int SeamstressCount, decimal ShiftMinutes, decimal? MinuteCost);

/// <summary>«Прогноз пошиття»: the queue in the order it is sewn.</summary>
/// <param name="Problem">Why no date is counted at all (capacity parameters missing), or null.</param>
public sealed record ForecastResponse(
    DateOnly StartDate,
    ForecastSettingsResponse? Settings,
    string? Problem,
    IReadOnlyList<ForecastRowResponse> Rows,
    bool CanReorder,
    int Version);

/// <summary>The new order of the queue; every listed order must be in the queue now.</summary>
public sealed record SaveForecastQueueInput(int Version, IReadOnlyList<int> OrderIds);
