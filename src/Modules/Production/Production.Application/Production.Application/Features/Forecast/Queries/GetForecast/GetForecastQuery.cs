using Production.Application.Abstractions;
using Production.Domain.Forecast;
using Production.Domain.Orders;
using Production.Application.Features.Forecast.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.PayrollParameters.Shared;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Forecast.Queries.GetForecast;

/// <summary>«Прогноз пошиття»: the queue with the approximate end of sewing of each order (PRD-D38, D39).</summary>
public sealed record GetForecastQuery(ProductionActor Actor) : IQuery<Result<ForecastResponse>>;
