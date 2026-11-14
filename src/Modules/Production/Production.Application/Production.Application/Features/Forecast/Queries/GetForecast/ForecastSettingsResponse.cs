namespace Production.Application.Features.Forecast.Queries.GetForecast;

/// <param name="MinuteCost">Only for Admin.</param>
public sealed record ForecastSettingsResponse(int SeamstressCount, decimal ShiftMinutes, decimal? MinuteCost);
