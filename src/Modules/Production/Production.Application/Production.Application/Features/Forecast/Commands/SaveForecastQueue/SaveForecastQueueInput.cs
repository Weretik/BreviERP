namespace Production.Application.Features.Forecast.Commands.SaveForecastQueue;

/// <summary>The new order of the queue; every listed order must be in the queue now.</summary>
public sealed record SaveForecastQueueInput(int Version, IReadOnlyList<int> OrderIds);
