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

public sealed class GetForecastValidator : AbstractValidator<GetForecastQuery>
{
    public GetForecastValidator() => RuleFor(x => x.Actor).ValidActor();
}

public sealed class GetForecastHandler(ForecastQueueReader reader, WorkCards cards,
    IPayrollParameterReader parameters, TimeProvider clock) : IQueryHandler<GetForecastQuery, Result<ForecastResponse>>
{
    public async ValueTask<Result<ForecastResponse>> Handle(GetForecastQuery query, CancellationToken cancellationToken)
    {
        var actor = query.Actor;
        var (queue, orders) = await reader.ReadAsync(track: false, cancellationToken);
        var sewing = orders.ToDictionary(x => x.Id,
            x => x.Items.Where(item => item.CurrentStage < ProductionStage.Ready).ToList());
        var done = await cards.DoneAsync([.. sewing.Values.SelectMany(x => x).Select(x => x.Id)], cancellationToken);
        var loads = orders.Select(order => new ForecastOrderLoad(order.Id, [
            .. sewing[order.Id].Select(item => new ForecastItemLoad(
                Math.Max(0, item.Quantity - done[item.Id].Sewn), item.SewingRate))
        ])).ToList();

        var settings = await ReadSettingsAsync(cancellationToken);
        var start = SewingForecast.NextWorkingDay(WorkshopDay.Today(clock));
        var lines = settings is { IsValid: true }
            ? SewingForecast.Calculate(loads, settings, start).ToDictionary(x => x.OrderId)
            : null;

        var rows = orders.Select(order =>
        {
            var remaining = loads.First(x => x.OrderId == order.Id).Items.Sum(x => x.Remaining);
            var line = lines?[order.Id];
            var dueDate = order.Items.Count > 0 ? order.Items.Min(x => x.DueDate) : order.OrderDate;
            return new ForecastRowResponse(order.Id, order.Number, order.ClientName, order.ManagerName, remaining,
                actor.SeesSewingRate ? line?.Minutes : null, line?.FinishOn,
                (line?.Status ?? (remaining == 0 ? ForecastStatus.Complete : ForecastStatus.Unknown)).ToString(),
                line?.Reason?.ToString(), dueDate, line?.FinishOn is { } finish && finish > dueDate);
        }).ToList();

        var settingsResponse = settings is not null && actor.SeesSewingRate
            ? new ForecastSettingsResponse(settings.SeamstressCount, settings.ShiftMinutes,
                actor.IsAdmin ? settings.MinuteCost : null)
            : null;
        return Result.Success(new ForecastResponse(start, settingsResponse,
            lines is null ? ForecastMessages.NoSettings : null, rows, actor.CanReorderForecast, queue?.Version ?? 0));
    }

    private async Task<ForecastSettings?> ReadSettingsAsync(CancellationToken cancellationToken)
    {
        var values = (await parameters.ReadAsync(
                [PayrollParameterCodes.SeamstressCount, PayrollParameterCodes.ShiftMinutes, PayrollParameterCodes.MinuteCost],
                cancellationToken))
            .ToDictionary(x => x.Code, x => x.Value);
        decimal? Value(string code) => values.GetValueOrDefault(code);
        if (Value(PayrollParameterCodes.SeamstressCount) is not { } count ||
            Value(PayrollParameterCodes.ShiftMinutes) is not { } shift ||
            Value(PayrollParameterCodes.MinuteCost) is not { } cost)
            return null;
        return new ForecastSettings((int)decimal.Floor(count), shift, cost);
    }
}
