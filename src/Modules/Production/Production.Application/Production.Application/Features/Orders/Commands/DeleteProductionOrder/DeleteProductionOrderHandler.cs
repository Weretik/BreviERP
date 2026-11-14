using Production.Application.Abstractions;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;

namespace Production.Application.Features.Orders.Commands.DeleteProductionOrder;

public sealed class DeleteProductionOrderHandler(IProductionDbContext db, IProductionStaffReader staff,
    TimeProvider clock) : ICommandHandler<DeleteProductionOrderCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteProductionOrderCommand command, CancellationToken cancellationToken)
    {
        if (!command.Actor.CanDelete) return Result.Forbidden(OrderMessages.CannotDelete);
        var order = await db.Orders.SingleOrDefaultAsync(x => x.Id == command.Id && x.DeletedAt == null,
            cancellationToken);
        if (order is null) return Result.NotFound(OrderMessages.NotFound);
        if (order.Version != command.Version) return Result.Conflict(OrderMessages.Stale);
        var actorName = await OrderActors.NameAsync(staff, command.Actor.UserId, cancellationToken);
        if (actorName is null) return Result.Forbidden(OrderMessages.ActorUnknown);
        order.Delete(actorName, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
