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

public sealed class SaveForecastQueueValidator : AbstractValidator<SaveForecastQueueCommand>
{
    public SaveForecastQueueValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.Version).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Input.OrderIds).NotNull().Must(x => x.Count <= ForecastQueue.MaxOrders);
            RuleFor(x => x.Input.OrderIds).Must(x => x.Distinct().Count() == x.Count)
                .WithMessage(ForecastMessages.Duplicate).When(x => x.Input.OrderIds is not null);
        });
    }
}

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
