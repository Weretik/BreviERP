namespace Production.Application.Features.Forecast.Queries.GetForecast;

/// <summary>«Прогноз пошиття»: the queue in the order it is sewn.</summary>
/// <param name="Problem">Why no date is counted at all (capacity parameters missing), or null.</param>
public sealed record ForecastResponse(
    DateOnly StartDate,
    ForecastSettingsResponse? Settings,
    string? Problem,
    IReadOnlyList<ForecastRowResponse> Rows,
    bool CanReorder,
    int Version);
