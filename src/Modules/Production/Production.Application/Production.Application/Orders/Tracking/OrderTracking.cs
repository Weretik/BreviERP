using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Domain.Orders;

namespace Production.Application.Orders.Tracking;

/// <summary>
/// The common steps of a change on the card: load the order, check its version and the person, apply, save, and
/// return the new card.
/// </summary>
public sealed class OrderTracking(IProductionDbContext db, IProductionStaffReader staff, OrderCards cards,
    TimeProvider clock)
{
    public async Task<Result<ProductionOrderResponse>> ChangeAsync(ProductionActor actor, int orderId, int version,
        Func<ProductionOrder, string?> check, Func<ProductionOrder, string, DateTimeOffset, Result?> apply,
        CancellationToken cancellationToken)
    {
        var order = await cards.Orders.SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order is null) return Result<ProductionOrderResponse>.NotFound(OrderMessages.NotFound);
        if (check(order) is { } forbidden) return Result<ProductionOrderResponse>.Forbidden(forbidden);
        if (order.Version != version) return Result<ProductionOrderResponse>.Conflict(OrderMessages.Stale);
        var actorName = await OrderActors.NameAsync(staff, actor.UserId, cancellationToken);
        if (actorName is null) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.ActorUnknown);

        var failed = apply(order, actorName, clock.GetUtcNow());
        if (failed is not null)
            return failed.Status == ResultStatus.NotFound
                ? Result<ProductionOrderResponse>.NotFound([.. failed.Errors])
                : Result<ProductionOrderResponse>.Conflict([.. failed.Errors]);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await cards.BuildAsync(order, actor, cancellationToken));
    }
}
