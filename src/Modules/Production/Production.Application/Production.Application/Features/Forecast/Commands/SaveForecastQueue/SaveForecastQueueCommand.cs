using Production.Application.Abstractions;
using Production.Domain.Forecast;
using Production.Application.Features.Forecast.Shared;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Forecast.Commands.SaveForecastQueue;

/// <summary>
/// The head of the workshop or Admin sets the order of «Прогноз пошиття» (PRD-D39). <see cref="SaveForecastQueueInput.Version"/>
/// guards against two people reordering at once.
/// </summary>
public sealed record SaveForecastQueueCommand(ProductionActor Actor, SaveForecastQueueInput Input)
    : ICommand<Result>;
