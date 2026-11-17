using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Responses;

namespace Production.Application.Features.Orders.Commands.DeleteProductionOrderFile;

public sealed class DeleteProductionOrderFileHandler(
    IProductionDbContext db, IProductionStaffReader staff, IProductionFileStorage storage, OrderCards cards,
    TimeProvider clock, ILogger<DeleteProductionOrderFileHandler> logger)
    : ICommandHandler<DeleteProductionOrderFileCommand, Result<ProductionOrderResponse>>
{
    public async ValueTask<Result<ProductionOrderResponse>> Handle(DeleteProductionOrderFileCommand command,
        CancellationToken cancellationToken)
    {
        var order = await cards.Orders.SingleOrDefaultAsync(x => x.Id == command.OrderId, cancellationToken);
        if (order is null) return Result<ProductionOrderResponse>.NotFound(OrderMessages.NotFound);
        if (!command.Actor.CanEdit(order)) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.CannotEdit);
        await db.Orders.Entry(order).Collection(x => x.Files).Query()
            .Where(x => x.Id == command.FileId).LoadAsync(cancellationToken);
        var actorName = await OrderActors.NameAsync(staff, command.Actor.UserId, cancellationToken);
        if (actorName is null) return Result<ProductionOrderResponse>.Forbidden(OrderMessages.ActorUnknown);

        var removed = order.RemoveFile(command.FileId, actorName, clock.GetUtcNow());
        if (removed.IsFailure) return removed.ToResult<ProductionOrderFile, ProductionOrderResponse>();
        var file = removed.Value;
        await db.SaveChangesAsync(cancellationToken);
        if (file.StorageKey is { } key)
        {
            try
            {
                await storage.DeleteAsync(key, CancellationToken.None);
            }
            catch (Exception error)
            {
                // The file is already gone from the order; a CDN leftover is harmless.
                logger.LogWarning(error, "CDN file {Key} of production order {Number} was not deleted", key, order.Number);
            }
        }
        return Result.Success(await cards.BuildAsync(order, command.Actor, cancellationToken));
    }
}
