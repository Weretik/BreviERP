using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Domain.Stock.Movements;
using Warehouse.Application.Features.Stock.Shared.Operations;
using BuildingBlocks.Application;

namespace Warehouse.Application.Features.Stock.Commands.RestoreFabricMovement;

public sealed class RestoreFabricMovementHandler(IWarehouseDbContext db, StockChanges changes,
    StockPairs pairs, StockOperations operations, MovementRevisions revisions)
    : ICommandHandler<RestoreFabricMovementCommand, Result<FabricStockOperationResponse>>
{
    public const string Kind = "movement-restore";

    public async ValueTask<Result<FabricStockOperationResponse>> Handle(RestoreFabricMovementCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        if (!command.Actor.CanMoveStock) return Result<FabricStockOperationResponse>.Forbidden(StockMessages.CannotMove);
        var hash = StockOperations.Hash(command.Actor.UserId, Kind, new { command.MovementId, input.OperationId });
        if (await operations.ReplayAsync(input.OperationId, hash, cancellationToken) is { } replay) return replay;
        var actorName = await changes.ActorNameAsync(command.Actor, cancellationToken);
        if (actorName is null) return Result<FabricStockOperationResponse>.Forbidden(StockMessages.CannotMove);
        var loaded = await revisions.LoadAsync(command.MovementId, input.ExpectedRevision, true, cancellationToken);
        if (!loaded.IsSuccess) return loaded.Map(_ => default(FabricStockOperationResponse)!);
        var movement = loaded.Value;

        var last = await Last(db, movement.Id, cancellationToken);
        if (last is null) return Result<FabricStockOperationResponse>.Conflict(StockMessages.NothingToRestore);
        if (last.After != movement.State) return Result<FabricStockOperationResponse>.Conflict(StockMessages.RestoreConflict);
        var from = movement.Stock;
        var to = last.Before.StockId == from.Id
            ? from
            : await db.FabricStocks.SingleAsync(x => x.Id == last.Before.StockId, cancellationToken);
        if (!FabricStock.IsValidMeters(to.Meters + last.Before.Delta - (ReferenceEquals(from, to) ? movement.Delta : 0)))
            return Result<FabricStockOperationResponse>.Invalid(new ValidationError("Meters", StockMessages.OutOfRange));

        var revision = to.Revise(movement, last.Before, FabricMovementRevisionAction.Restore, null, actorName, pairs.Now,
            input.OperationId, last);
        if (revision.IsFailure) return revision.Error!.ToResult<FabricStockOperationResponse>();
        return await revisions.SaveAsync(command.Actor, input.OperationId, Kind, hash, revision.Value, from, to,
            cancellationToken);
    }

    /// <summary>The newest edit or void of the movement that no restore took back.</summary>
    public static Task<FabricMovementRevision?> Last(IWarehouseDbContext db, int movementId,
        CancellationToken cancellationToken) =>
        db.FabricMovementRevisions
            .Where(x => x.MovementId == movementId && !x.IsUndone && x.Action != FabricMovementRevisionAction.Restore)
            .OrderByDescending(x => x.Number).FirstOrDefaultAsync(cancellationToken);
}
