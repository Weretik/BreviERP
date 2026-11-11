using Warehouse.Application.Abstractions;
using Warehouse.Application.Access;
using Warehouse.Application.Contracts;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock.Revisions;

/// <summary>
/// «Скасувати останню правку»: the state before the last edit or void comes back, only while the movement still is
/// what that revision left, so a restore never overwrites someone else's newer change.
/// </summary>
public sealed record RestoreFabricMovementCommand(WarehouseActor Actor, int MovementId,
    RestoreFabricMovementInput Input) : ICommand<Result<FabricStockOperationResponse>>;

public sealed class RestoreFabricMovementCommandValidator : AbstractValidator<RestoreFabricMovementCommand>
{
    public RestoreFabricMovementCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.MovementId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.ExpectedRevision).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class RestoreFabricMovementCommandHandler(IWarehouseDbContext db, StockChanges changes,
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
        return await revisions.SaveAsync(command.Actor, input.OperationId, Kind, hash, revision, from, to,
            cancellationToken);
    }

    /// <summary>The newest edit or void of the movement that no restore took back.</summary>
    public static Task<FabricMovementRevision?> Last(IWarehouseDbContext db, int movementId,
        CancellationToken cancellationToken) =>
        db.FabricMovementRevisions
            .Where(x => x.MovementId == movementId && !x.IsUndone && x.Action != FabricMovementRevisionAction.Restore)
            .OrderByDescending(x => x.Number).FirstOrDefaultAsync(cancellationToken);
}
