using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.ReverseFabricMovement;

public sealed class ReverseFabricMovementHandler(IWarehouseDbContext db, StockChanges changes, StockPairs pairs)
    : ICommandHandler<ReverseFabricMovementCommand, Result<FabricStockOperationResponse>>
{
    public async ValueTask<Result<FabricStockOperationResponse>> Handle(ReverseFabricMovementCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanMoveStock) return Result<FabricStockOperationResponse>.Forbidden(StockMessages.CannotMove);
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<FabricStockOperationResponse>.Forbidden(StockMessages.CannotMove);
        var movement = await db.FabricMovements.SingleOrDefaultAsync(x => x.Id == command.MovementId, cancellationToken);
        if (movement is null) return Result<FabricStockOperationResponse>.NotFound(StockMessages.MovementNotFound);
        if (movement.IsReversed) return Result<FabricStockOperationResponse>.Conflict(StockMessages.AlreadyReversed);
        if (movement.IsVoided) return Result<FabricStockOperationResponse>.Conflict(StockMessages.Voided);
        if (!CanReverse(movement)) return Result<FabricStockOperationResponse>.Conflict(StockMessages.CannotReverse);

        var line = await db.FabricStocks.SingleAsync(x => x.Id == movement.StockId, cancellationToken);
        var all = await db.FabricMovements.Where(x => x.StockId == line.Id).ToListAsync(cancellationToken);
        line.Reverse(movement, new FabricMovementDetails(pairs.Today, command.Input.Reason), actorName, pairs.Now);
        line.Restate(all.Concat(line.Movements));
        var saved = await changes.SaveAsync(cancellationToken);
        if (!saved.IsSuccess) return Result<FabricStockOperationResponse>.Conflict([.. saved.Errors]);

        var pair = await pairs.LoadAsync(line.FabricCode, line.Color, cancellationToken);
        return Result.Success(new FabricStockOperationResponse(null, pair.Code, line.Color, pair.Meters, false));
    }

    /// <summary>Openings, receipts, corrections and issues without an order may be reversed here.</summary>
    public static bool CanReverse(FabricMovement x) => !x.IsReversed && !x.IsVoided &&
        (x.Kind is FabricMovementKind.Opening or FabricMovementKind.Receipt or FabricMovementKind.Adjustment ||
         x.Kind == FabricMovementKind.Issue && x.ItemId is null);
}
