using Production.Application.Abstractions;
using Production.Domain.Forecast;
using Production.Application.Features.Forecast.Shared;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Forecast.Commands.SaveForecastQueue;

public sealed class SaveForecastQueueHandler(IProductionDbContext db, ForecastQueueReader reader,
    IProductionStaffReader staff, TimeProvider clock) : ICommandHandler<SaveForecastQueueCommand, Result>
{
    public async ValueTask<Result> Handle(SaveForecastQueueCommand command, CancellationToken cancellationToken)
    {
        if (!command.Actor.CanReorderForecast) return Result.Forbidden(ForecastMessages.CannotReorder);
        var (queue, orders) = await reader.ReadAsync(track: true, cancellationToken);
        if ((queue?.Version ?? 0) != command.Input.Version) return Result.Conflict(ForecastMessages.Stale);
        var inQueue = orders.Select(x => x.Id).ToHashSet();
        if (!command.Input.OrderIds.All(inQueue.Contains)) return Result.Conflict(ForecastMessages.NotInQueue);
        var actorName = await OrderActors.NameAsync(staff, command.Actor.UserId, cancellationToken);
        if (actorName is null) return Result.Forbidden(ForecastMessages.CannotReorder);

        var now = clock.GetUtcNow();
        if (queue is null)
        {
            queue = new ForecastQueue(now);
            db.ForecastQueues.Add(queue);
        }

        if (queue.Set(command.Input.OrderIds, actorName, now).FailureOrNull() is { } invalid) return invalid;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
